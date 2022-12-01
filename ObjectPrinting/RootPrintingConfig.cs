using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

public class RootPrintingConfig<TOwner> : PrintingConfig<TOwner>, IInternalPrintingConfigStorage
{
    internal RootPrintingConfig() : base(null)
    {
    }

    public CyclicInheritanceHandler CyclicInheritanceHandler { get; set; } = CyclicInheritanceHandler.IgnoreMembers;

    public HashSet<Type> DictionaryTypeCheckIgnores { get; } = new();

    public HashSet<Type> CollectionTypeCheckIgnores { get; } = new();

    public LinkedList<(Type Type, Func<object, string> Serializer)>
        AssignableTypeSerializers { get; } = new(
        new (Type Type, Func<object, string> Serializer)[]
        {
            (typeof(bool), o => o.ToString() ?? string.Empty),
            (typeof(string), o => o.ToString() ?? string.Empty),
            (typeof(IFormattable), o => ((IFormattable)o).ToString(null, CultureInfo.CurrentCulture))
        });

    public HashSet<Type> IgnoredTypesFromAssignableCheck { get; } = new();
    public HashSet<MemberInfo> MemberExcluding { get; } = new();

    public Dictionary<MemberInfo, Func<object, string>> MemberSerializers { get; } = new();

    public HashSet<Type> TypeExcluding { get; } = new();

    public Dictionary<Type, Func<object, string>> TypeSerializers { get; } = new();

    public void PrintToString(object obj, int nestingLevel, StringBuilder stringBuilder,
        HashSet<object> cyclicInheritanceIgnoredObjects)
    {
        nestingLevel = nestingLevel + 1;
        var type = obj.GetType();
        stringBuilder.AppendTypeName(type);

        if (CyclicInheritanceHandler == CyclicInheritanceHandler.IgnoreMembers && type.IsClass)
        {
            if (cyclicInheritanceIgnoredObjects.Contains(obj))
                return;
            cyclicInheritanceIgnoredObjects.Add(obj);
        }

        var internalPrintingConfig = this as IInternalPrintingConfig<TOwner>;
        var root = internalPrintingConfig.GetRoot();


        if (!DictionaryTypeCheckIgnores.Contains(type) && type.IsGenericAssignable(typeof(IReadOnlyDictionary<,>)))
        {
            var enumerable = (IEnumerable)obj;
            root.ConsumeDictionaryItems(enumerable, nestingLevel, stringBuilder, '\t', cyclicInheritanceIgnoredObjects);
            return;
        }

        DictionaryTypeCheckIgnores.Add(type);

        if ((!CollectionTypeCheckIgnores.Contains(type) && type.IsAssignableTo(typeof(ICollection))) ||
            type.IsGenericAssignable(typeof(IReadOnlyCollection<>)))
        {
            var enumerable = (IEnumerable)obj;
            root.ConsumeItems(enumerable, nestingLevel, stringBuilder, '\t', cyclicInheritanceIgnoredObjects);
            return;
        }

        CollectionTypeCheckIgnores.Add(type);

        foreach (var propertyInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            root.ConsumeProperty(obj, nestingLevel, stringBuilder, propertyInfo, '\t',
                cyclicInheritanceIgnoredObjects);

        foreach (var fieldInfo in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            root.ConsumeField(obj, nestingLevel, stringBuilder, fieldInfo, '\t',
                cyclicInheritanceIgnoredObjects);
    }
}

public static class TypeExtensions
{
    public static void AppendTypeName(this StringBuilder stringBuilder, Type type, bool newTypeName = true)
    {
        if (type.IsGenericType)
        {
            var lastIndexOf = type.Name.LastIndexOf('`');
            var nameWithoutGenericParametersCount = type.Name[..lastIndexOf];
            stringBuilder.Append(nameWithoutGenericParametersCount)
                .Append('<');
            foreach (var typeArgument in type.GenericTypeArguments)
            {
                stringBuilder.AppendTypeName(typeArgument, false);
                stringBuilder.Append(", ");
            }

            stringBuilder.Length -= 2;
            stringBuilder.Append('>');
            if (newTypeName)
                stringBuilder.AppendLine();
        }
        else
        {
            if (newTypeName)
                stringBuilder.AppendLine(type.Name);
            else stringBuilder.Append(type.Name);
        }
    }

    public static bool IsGenericAssignable(this Type original, Type baseTypeGenericType, int maxDeep = 3)
    {
        var current = original;
        var deep = 0;
        while (current != typeof(object) && deep++ < maxDeep)
        {
            if (current.IsGenericType)
            {
                var currentGenericTypeArguments = current.GenericTypeArguments;
                try
                {
                    var baseType = baseTypeGenericType.MakeGenericType(currentGenericTypeArguments);
                    if (original.IsAssignableTo(baseType))
                        return true;
                }
                catch
                {
                    // NOOP
                }
            }

            current = current.BaseType!;
        }

        return false;
    }
}