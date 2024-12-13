using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private readonly HashSet<Type> typesToExclude = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<string> propertiesToExclude = [];
    private readonly HashSet<object> processedObjects = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<Type, CultureInfo> cultureSerializers = [];
    private readonly Dictionary<Type, List<Delegate>> typeSerializers = [];
    private readonly Dictionary<string, List<Delegate>> propertySerializers = [];
    private int MaxNestingLevel { get; set; } = 5;

    public PrintingConfig<TOwner> SetMaxNestingLevel(int maxNestingLevel)
    {
        if (maxNestingLevel < 0)
        {
            throw new ArgumentException("Max nesting level must be greater than or equal to 0.");
        }

        MaxNestingLevel = maxNestingLevel;
        return this;
    }

    public PrintingConfig<TOwner> Exclude<TPropType>()
    {
        typesToExclude.Add(typeof(TPropType));
        return this;
    }

    public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> propertySelector)
    {
        propertiesToExclude.Add(GetPropertyMemberInfo(propertySelector).Name);
        return this;
    }

    public IPropertyPrintingConfig<TPropType, TOwner> PrintPropertySettings<TPropType>(
        Expression<Func<TOwner, TPropType>> propertySelector)
    {
        return new PropertyPrintingConfig<TPropType, TOwner>(this, GetPropertyMemberInfo(propertySelector).Name);
    }

    public ITypePrintingConfig<TOwner, TPropType> PrintSettings<TPropType>()
    {
        return new TypePrintingConfig<TOwner, TPropType>(this);
    }

    public PrintingConfig<TOwner> UseCulture<TPropType>(CultureInfo culture) where TPropType : IFormattable
    {
        cultureSerializers[typeof(TPropType)] = culture;
        return this;
    }

    internal void AddTypeSerializer<TPropType>(Func<TPropType, string> serializeFunc)
    {
        if (typeSerializers.TryGetValue(typeof(TPropType), out var serializers))
        {
            serializers.Add(serializeFunc);
            return;
        }
        typeSerializers[typeof(TPropType)] = [serializeFunc];
    }

    internal void AddPropertySerializer<TPropType>(string propertyName, Func<TPropType, string> serializeFunc)
    {
        if (propertySerializers.TryGetValue(propertyName, out var serializers))
        {
            serializers.Add(serializeFunc);
            return;
        }
        propertySerializers[propertyName] = [serializeFunc];
    }

    public string PrintToString(TOwner obj)
    {
        return PrintToString(obj, 1);
    }

    private string PrintToString(object? obj, int nestingLevel)
    {
        if (obj is null)
        {
            return "null";
        }

        if (processedObjects.Contains(obj))
        {
            return "It is not possible to print an object with a circular reference.";
        }

        var type = obj.GetType();

        if (typeSerializers.TryGetValue(type, out var serializers))
        {
            var result = serializers.Aggregate(obj, (current, serializer) => serializer.DynamicInvoke(current)!);

            return result.ToString()!;
        }

        if (type.IsPrimitive || type == typeof(string) || type == typeof(Guid))
        {
            return type == typeof(string) ? $"\"{obj}\"" : obj.ToString()!;
        }
        
        if (nestingLevel > MaxNestingLevel)
        {
            return $"The maximum recursion depth has been reached: {MaxNestingLevel}.";
        }
        
        processedObjects.Add(obj);
        if (obj is IEnumerable enumerable && type != typeof(string))
        {
            processedObjects.Clear();
            return SerializeEnumerable(enumerable, nestingLevel);
        }
        
        var indentation = new string('\t', nestingLevel);
        var stringBuilderResult = new StringBuilder();
        stringBuilderResult.AppendLine($"{type.Name}");

        foreach (var propertyInfo in type.GetProperties())
        {
            if (propertiesToExclude.Contains(propertyInfo.Name) || typesToExclude.Contains(propertyInfo.PropertyType))
            {
                continue;
            }

            var propertyValue = propertyInfo.GetValue(obj);
            var propertyType = propertyInfo.PropertyType;
            var propertyName = propertyInfo.Name;

            stringBuilderResult.AppendLine(GetSerializeString(nestingLevel, propertyName, indentation, propertyValue,
                propertyType));
        }

        return stringBuilderResult.ToString();
    }

    private string GetSerializeString(
        int nestingLevel,
        string propertyName,
        string indentation,
        object? propertyValue,
        Type propertyType
    )
    {
        if (propertySerializers.TryGetValue(propertyName, out var serializers))
        {
            var result =
                serializers.Aggregate(propertyValue, (current, serializer) => serializer.DynamicInvoke(current));

            return $"{indentation}{propertyName} = {result!}";
        }

        if (cultureSerializers.TryGetValue(propertyType, out var culture))
        {
            var propertyValueFormattable = propertyValue as IFormattable;

            return $"{indentation}{propertyName} = {propertyValueFormattable!.ToString(null, culture)}";
        }

        return $"{indentation}{propertyName} = {PrintToString(propertyValue, nestingLevel + 1)}";
    }

    private string SerializeEnumerable(IEnumerable enumerable, int nestingLevel)
    {
        var objects = enumerable.Cast<object>().ToList();

        if (objects.Count == 0)
        {
            return "{}";
        }

        var indentation = new string('\t', nestingLevel);
        var serializeResult = new StringBuilder();
        serializeResult.AppendLine("{");

        if (enumerable is IDictionary dictionary)
        {
            foreach (var key in dictionary.Keys)
            {
                var value = dictionary[key];
                var keyConvert = PrintToString(key, nestingLevel);
                var valueConvert = PrintToString(value, nestingLevel + 1);

                serializeResult.Append($"{indentation}{keyConvert}: {valueConvert}" + Environment.NewLine);
            }
        }
        else
        {
            for (var i = 0; i < objects.Count; i++)
            {
                var obj = objects[i];
                serializeResult.Append($"{indentation}{PrintToString(obj, nestingLevel + 1)}");
                if (i < objects.Count - 1)
                {
                    serializeResult.Append($"{indentation},");
                    serializeResult.Append(Environment.NewLine);
                }
            }
        }

        serializeResult.AppendLine($"{indentation}}}");

        return serializeResult.ToString();
    }

    private static MemberInfo GetPropertyMemberInfo<TPropType>(Expression<Func<TOwner, TPropType>> propertySelector)
    {
        return propertySelector.Body switch
        {
            MemberExpression memberExpression => memberExpression.Member,
            UnaryExpression { Operand: MemberExpression operand } => operand.Member,
            var _ => throw new ArgumentException("Invalid property selector expression")
        };
    }
}