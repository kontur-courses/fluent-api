using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Solved;

public static class ObjectPrinter
{
    private static readonly HashSet<Type> _finalTypes = new()
    {
        typeof(int), typeof(double), typeof(float), typeof(string),
        typeof(DateTime), typeof(TimeSpan)
    };

    public static PrintingConfig<T> For<T>()
    {
        return new PrintingConfig<T>();
    }

    public static string PrintToString<TOwner>(this object obj, PrintingConfig<TOwner> config)
    {
        return PrintToString(obj, config, 0, new HashSet<object>());
    }

    private static string PrintToString<TOwner>(object obj, PrintingConfig<TOwner> config, int nestingLevel,
        HashSet<object> printedObjects)
    {
        printedObjects.Add(obj);
        if (obj is null)
            return "null";

        if (_finalTypes.Contains(obj.GetType()))
            return obj.ToString();

        var stringBuilder = new StringBuilder();
        foreach (var memberInfo in config.Members)
        {
            var memberType = GetTypeFromMemberInfo(memberInfo);
            var memberValue = GetValueFromMemberInfo(obj, memberInfo);

            if (memberType.IsValueType == false && memberType != typeof(string) &&
                memberValue is not null && printedObjects.Contains(memberValue))
                continue;
            printedObjects.Add(memberValue);

            string printedMember;
            if (config.AlternativePropertiesPrints.ContainsKey(memberInfo.Name))
                printedMember = config.AlternativePropertiesPrints[memberInfo.Name](memberValue);
            else if (config.AlternativeTypesPrints.ContainsKey(memberType))
                printedMember = config.AlternativeTypesPrints[memberType](memberValue);
            else
                printedMember = PrintToString(memberValue, config, nestingLevel + 1,
                    new HashSet<object>(printedObjects));
            AddLineToPrint(stringBuilder, nestingLevel, memberInfo.Name, printedMember);
        }

        return stringBuilder.ToString();
    }

    private static Type GetTypeFromMemberInfo(MemberInfo memberInfo)
    {
        return memberInfo switch
        {
            FieldInfo fieldInfo => fieldInfo.FieldType,
            PropertyInfo propertyInfo => propertyInfo.PropertyType,
            _ => throw new ArgumentException("Member is not a field or a property!")
        };
    }

    private static object? GetValueFromMemberInfo(object obj, MemberInfo memberInfo)
    {
        return memberInfo switch
        {
            FieldInfo fieldInfo => fieldInfo.GetValue(obj),
            PropertyInfo propertyInfo => propertyInfo.GetValue(obj),
            _ => throw new ArgumentException("Member is not a field or a property!")
        };
    }

    private static void AddLineToPrint(StringBuilder stringBuilder, int shiftLevel, string name, string value)
    {
        var shift = new string('\t', shiftLevel);
        stringBuilder.Append($"{Environment.NewLine}{shift}{name} = {value}");
    }
}