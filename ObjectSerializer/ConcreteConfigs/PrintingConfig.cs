using System;
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
    private readonly HashSet<MemberInfo> excludedMemberInfos = new();

    private readonly HashSet<Type> excludedTypes = new();

    //Func печати для определенного поля объекта
    public readonly Dictionary<MemberInfo, Func<object, string>> MemberInfosPrintFunc = new();

    public readonly Dictionary<MemberInfo, (int Start, int Length)> FieldLengths = new();

    public readonly Dictionary<Type, CultureInfo> TypeCultureInfos = new();

    //Func печати для определенного типа
    public readonly Dictionary<Type, Func<object, string>> TypePrintFunc = new();

    //Объекты которые мы уже обработали
    private readonly HashSet<object> processedObjects = new();

    //Не примитивные, не 'вложенные' типы,
    private readonly HashSet<Type> nonPrimitiveFinalTypes = new()
    {
        typeof(DateTime), typeof(TimeSpan), typeof(Guid), typeof(string)
    };

    #region Print methods

    public ITypePrintConfig<TOwner, TType> Print<TType>()
    {
        return new TypePrintConfig<TOwner, TType>(this);
    }

    public IFieldPrintConfig<TOwner, TField> Print<TField>(Expression<Func<TOwner, TField>> selectMember)
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

        if (IsFinalType(objType))
            return GetStringOfFinalType(obj);

        //cycles
        if(processedObjects.Contains(obj))
            return "!Циклическая ссылка!";

        processedObjects.Add(obj);

        return obj switch
        {
            IDictionary dictionary => PrintDictionary(dictionary, nestingLevel),
            IEnumerable collection => PrintCollection(collection, nestingLevel),
            _ => PrintClassMembers(obj, nestingLevel)
        };
    }

    private string PrintDictionary(IDictionary dictionary, int nestingLevel)
    {
        var stringBuilder = new StringBuilder();
        var indentation = CreateIndentation(nestingLevel);
        stringBuilder.Append(string.Concat(indentation, "Dictionary"));
        nestingLevel++;
        indentation = CreateIndentation(nestingLevel);

        foreach (DictionaryEntry entry in dictionary)
        {
            var key = entry.Key;
            var value = entry.Value;

            stringBuilder.AppendLine();
            var keyPrint = PrintToString(key, nestingLevel);

            if (IsFinalType(value.GetType()))
            {
                stringBuilder.Append(indentation + keyPrint + " : ");

                var valuePrint = PrintToString(value, 0);

                stringBuilder.Append(valuePrint);
            }
            else
            {
                stringBuilder.AppendLine(indentation + keyPrint + " : ");
                var valuePrint = PrintToString(value, nestingLevel + 1);

                stringBuilder.Append(valuePrint);
            }
        }

        return stringBuilder.ToString();
    }

    private string PrintCollection(IEnumerable collection, int nestingLevel)
    {
        var stringBuilder = new StringBuilder();
        var indentation = CreateIndentation(nestingLevel);
        stringBuilder.Append(string.Concat(indentation, "Collection"));

        nestingLevel++;
        indentation = CreateIndentation(nestingLevel);

        foreach (var item in collection)
        {
            stringBuilder.AppendLine();
            var itemPrint = PrintToString(item, nestingLevel).TrimStart();
            stringBuilder.Append(String.Concat(indentation, itemPrint));
        }

        return stringBuilder.ToString();
    }

    private string PrintClassMembers(object obj, int nestingLevel)
    {
        var stringBuilder = new StringBuilder();
        var objType = obj.GetType();
        var indentation = CreateIndentation(nestingLevel);
        stringBuilder.Append(string.Concat(indentation, objType.Name));
        nestingLevel++;
        indentation = CreateIndentation(nestingLevel);

        foreach (var memberInfo in GetMembersToSerialize(objType))
        {
            if (!excludedMemberInfos.Contains(memberInfo) && !excludedTypes.Contains(GetMemberType(memberInfo)))
            {
                var memberValue = GetMemberValue(obj, memberInfo);
                var memberString = GetMemberValueString(memberValue, memberInfo, nestingLevel + 1);
                stringBuilder.AppendLine();
                if (memberString == "!Циклическая ссылка!")
                {
                    stringBuilder.Append(string.Concat(indentation,
                        memberInfo.Name, " = ", memberString));
                }
                else if (IsFinalType(memberValue.GetType()))
                    stringBuilder.Append(string.Concat(indentation,
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
        else if (FieldLengths.TryGetValue(memberInfo, out var range))
            stringValue = Trim(memberValue?.ToString()!, range.Start, range.Length);
        else if(TypeCultureInfos.TryGetValue(memberType, out var culture))
            stringValue = string.Format(culture, memberValue.ToString());

        if (stringValue == null)
            stringValue = PrintToString(memberValue, nestingLevel);
        else if (nonPrimitiveFinalTypes.Contains(memberValue.GetType()))
            stringValue = $"\"{stringValue}\"";

        return stringValue;
    }

    private string CreateIndentation(int nestingLevel)
    {
        return new string('\t', nestingLevel);
    }

    private string GetStringOfFinalType(object obj)
    {
        var type = obj.GetType();

        if(type.IsPrimitive)
            return obj.ToString();
        if (type.IsEnum)
            return obj.ToString();
        if (nonPrimitiveFinalTypes.Contains(type))
            return $"\"{obj}\"";

        throw new ArgumentException($"Переданный {obj.GetType()} не принадлежит к конечным типам");
    }

    #endregion

    #region HelpersMethods

    private bool IsFinalType(Type type)
    {
        return type.IsPrimitive || type.IsEnum || nonPrimitiveFinalTypes.Contains(type);
    }

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
        if (member is FieldInfo fieldInfo)
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

    private string Trim(string str, int start, int len)
    {
        if (str == null) throw new ArgumentNullException(nameof(str));

        return str.Substring(start, len);
    }

    #endregion
}