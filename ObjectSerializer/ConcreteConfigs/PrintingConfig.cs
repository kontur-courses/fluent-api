using System.Collections;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectSerializer.Interfaces;

namespace ObjectSerializer.ConcreteConfigs;

public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
{
    private HashSet<MemberInfo> excludedMemberInfos = new();

    private HashSet<Type> excludedTypes = new();

    //Func печати для определенного поля объекта
    public Dictionary<MemberInfo, Func<object, string>> MemberInfosPrintFunc = new();

    public Dictionary<MemberInfo, uint> FieldLengths = new();

    public Dictionary<Type, CultureInfo> TypeCultureInfos = new();

    //Func печати для определенного типа
    public Dictionary<Type, Func<object, string>> TypePrintFunc = new();

    //Объекты которые мы уже обработали
    private HashSet<object> processedObjects = new();

    //Конечные, не 'вложенные' типы
    private HashSet<Type> finalTypes = new()
    {
        typeof(int), typeof(double), typeof(float), typeof(string),
        typeof(DateTime), typeof(TimeSpan), typeof(Guid)
    };

    #region For methods

    public ITypePrintConfig<TOwner, TType> For<TType>()
    {
        return new TypePrintConfig<TOwner, TType>(this);
    }

    public IFieldPrintConfig<TOwner, TField> For<TField>(Expression<Func<TOwner, TField>> selectMember)
    {
        var memberInfo = GetMemberInfo(selectMember);

        return new FieldPrintConfig<TOwner, TField>(this, memberInfo);
    }

    #endregion

    #region ExcludeMethods

    public IPrintingConfig<TOwner> Exclude<TType>()
    {
        excludedTypes.Add(typeof(TType));

        return this;
    }

    public IPrintingConfig<TOwner> Exclude<TField>(Expression<Func<TOwner, TField>> selectMember)
    {
        var memberInfo = GetMemberInfo(selectMember);

        excludedMemberInfos.Add(memberInfo);

        return this;
    }

    #endregion

    #region PrintToString

    public string PrintToString(TOwner obj)
    {
        if (obj == null) 
            throw new ArgumentNullException(nameof(obj));

        return PrintToString(obj, 0);
    }

    private string PrintToString(object obj, int nestingLevel)
    {
        //TODO apply configurations
        if (obj == null)
            return "null" + Environment.NewLine;

        var objType = obj.GetType(); 

        if (finalTypes.Contains(objType))
            return GetStringOfFinalType(obj);

        //cycles
        if(processedObjects.Contains(obj))
            return String.Empty;

        processedObjects.Add(obj);

        return obj switch
        {
            IDictionary dictionary => PrintDictionary(dictionary, nestingLevel),
            ICollection collection => PrintCollection(collection, nestingLevel),
            _ => PrintClassMembers(obj, nestingLevel)
        };
    }

    private string PrintDictionary(IDictionary dictionary, int nestingLevel)
    {
        var stringBuilder = new StringBuilder();
        var indentation = CreateIndentation(nestingLevel);
        stringBuilder.AppendLine(string.Concat(indentation, "Dictionary"));
        nestingLevel++;
        indentation = CreateIndentation(nestingLevel);

        foreach (DictionaryEntry entry in dictionary)
        {
            var key = entry.Key;
            var value = entry.Value;

            var keyPrint = PrintToString(key, nestingLevel + 1);

            stringBuilder.Append(indentation + keyPrint + " : ");

            var valuePrint = PrintToString(value, nestingLevel - 1);

            stringBuilder.AppendLine(valuePrint.TrimStart('\t'));
        }

        return stringBuilder.ToString();
    }

    private string PrintCollection(ICollection collection, int nestingLevel)
    {
        var stringBuilder = new StringBuilder();
        var indentation = CreateIndentation(nestingLevel);
        stringBuilder.AppendLine(string.Concat(indentation, "Collection"));
        nestingLevel++;
        indentation = CreateIndentation(nestingLevel);

        foreach (var item in collection)
        {
            var itemPrint = PrintToString(item, nestingLevel);
            stringBuilder.Append(itemPrint);
        }

        return stringBuilder.ToString();
    }

    private string PrintClassMembers(object obj, int nestingLevel)
    {
        var stringBuilder = new StringBuilder();
        var objType = obj.GetType();
        var indentation = CreateIndentation(nestingLevel);
        stringBuilder.AppendLine(string.Concat(indentation, objType.Name));
        nestingLevel++;
        indentation = CreateIndentation(nestingLevel);

        foreach (var memberInfo in GetMembersToSerialize(objType))
        {
            if (!excludedMemberInfos.Contains(memberInfo) && !excludedTypes.Contains(GetMemberType(memberInfo)))
            {
                var memberValue = GetMemberValue(obj, memberInfo);
                var memberString = GetMemberValueString(memberValue, memberInfo, nestingLevel + 1);

                if(memberString == String.Empty)
                    continue;
                if(finalTypes.Contains(memberValue.GetType()))
                    stringBuilder.AppendLine(string.Concat(indentation,
                    memberInfo.Name, " = ", memberString));
                else
                {
                    stringBuilder.AppendLine(string.Concat(indentation, memberInfo.Name, " = "));
                    stringBuilder.Append(memberString);
                }
            }
        }

        return stringBuilder.ToString();
    }

    private string GetMemberValueString(object memberValue, MemberInfo memberInfo, int nestingLevel)
    {
        var memberType = GetMemberType(memberInfo);
        string stringValue = null;

        if (MemberInfosPrintFunc.TryGetValue(memberInfo, out var printFieldFunc))
            stringValue = printFieldFunc(memberValue);
        else if (TypePrintFunc.TryGetValue(memberType, out var printTypeFunc))
            stringValue = printTypeFunc(memberValue);
        else if (FieldLengths.TryGetValue(memberInfo, out var fieldLength))
            stringValue = TrimToLength(memberValue?.ToString()!, fieldLength);
        else if(TypeCultureInfos.TryGetValue(memberType, out var culture))
            stringValue = string.Format(culture, memberValue.ToString());

        if (stringValue == null)
            stringValue = PrintToString(memberValue, nestingLevel);
        else if (!numericTypes.Contains(memberValue.GetType()))
            stringValue = $"\"{stringValue}\"";

        return stringValue;
    }

    private string CreateIndentation(int nestingLevel)
    {
        return new string('\t', nestingLevel);
    }

    private HashSet<Type> numericTypes = new HashSet<Type>()
    {
        typeof(int), typeof(double), typeof(float)
    };

    private string GetStringOfFinalType(object obj)
    {
        return obj switch
        {
            int i => i.ToString(),
            double d => d.ToString(),
            float f => f.ToString(),
            string s => $"\"{s}\"",
            DateTime dt => $"\"{dt}\"",
            TimeSpan ts => $"\"{ts}\"",
            Guid g => $"\"{g}\"",
            _ => throw new ArgumentException($"Переданный {obj.GetType()} не принадлежит к конечным типам")
        };
    }

    #endregion

    #region HelpersMethods

    private MemberInfo GetMemberInfo<TSomeField>(Expression<Func<TOwner, TSomeField>> selectMember)
    {
        if (selectMember.Body is not MemberExpression memberExpression)
            throw new ArgumentException();

        return memberExpression.Member;
    }

    private Type GetMemberType(MemberInfo member)
    {
        if (member is PropertyInfo propertyInfo)
        {
            return propertyInfo.PropertyType;
        }
        else if (member is FieldInfo fieldInfo)
        {
            return fieldInfo.FieldType;
        }

        throw new ArgumentException($"Поле для получения типа члена класса должно быть или PropertyInfo или FieldInfo");
    }

    private object GetMemberValue(object obj, MemberInfo member)
    {
        if (member is PropertyInfo propertyInfo)
        {
            return propertyInfo.GetValue(obj)!;
        }
        else if (member is FieldInfo fieldInfo)
        {
            return fieldInfo.GetValue(obj)!;
        }

        throw new ArgumentException($"Поле для получения значения члена класса должно быть или PropertyInfo или FieldInfo");
    }

    private IEnumerable<MemberInfo> GetMembersToSerialize(Type type)
    {
        return type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .Where(m => m is FieldInfo || m is PropertyInfo);
    }

    private string TrimToLength(string str, uint len)
    {
        if (str == null) throw new ArgumentNullException(nameof(str));
        if (len == 0) return string.Empty;
        if (str.Length <= len) return str;

        return str.Substring(0, (int)len);
    }

    #endregion
}