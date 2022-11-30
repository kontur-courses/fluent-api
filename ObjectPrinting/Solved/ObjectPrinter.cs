using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectPrinting.Solved;

public static class ObjectPrinter
{
    private static readonly HashSet<Type> _finalTypes = new()
    {
        typeof(int), typeof(double), typeof(float), typeof(string),
        typeof(DateTime), typeof(TimeSpan)
    };

    public static string PrintToString<TOwner>(this TOwner obj, PrintingConfig<TOwner> config)
    {
        return PrintToString(obj, config, 0, new HashSet<object>());
    }

    public static string PrintToString<TOwner>(ICollection collectionOfObjects, PrintingConfig<TOwner> config)
    {
        return PrintToString(collectionOfObjects, config, 0, new HashSet<object>());
    }

    private static string PrintToString<TOwner>(ICollection collectionOfObjects,
        PrintingConfig<TOwner> config, int shiftLevel, HashSet<object> printedObjects)
    {
        var shift = new string('\t', shiftLevel);

        var stringBuilder = new StringBuilder(Environment.NewLine);
        foreach (var obj in collectionOfObjects)
        {
            var printedObject = PrintToString(obj, config, shiftLevel, printedObjects);
            stringBuilder.Append($"{shift}{printedObject}{Environment.NewLine}");
        }

        return stringBuilder.ToString();
    }

    private static string PrintToString<TOwner>(object obj, PrintingConfig<TOwner> config, int nestingLevel,
        HashSet<object> printedObjects)
    {
        printedObjects.Add(obj);
        if (obj is null)
            return "null";
        if (_finalTypes.Contains(obj.GetType()))
            return obj.ToString();
        if (obj is ICollection collection)
            return PrintToString(collection, config, nestingLevel, printedObjects);

        var stringBuilder = new StringBuilder();
        var members = MemberInfoHelper.GetAllPropertiesAndFields(obj.GetType());

        foreach (var memberInfo in members)
        {
            if (config.ExcludedMembers.Contains(memberInfo))
                continue;
            var memberType = MemberInfoHelper.GetTypeFromMemberInfo(memberInfo);
            var memberValue = MemberInfoHelper.GetValueFromMemberInfo(obj, memberInfo);

            if (memberType.IsValueType == false && memberType != typeof(string) &&
                memberValue is not null && printedObjects.Contains(memberValue))
                continue;

            string printedMember;
            if (obj is TOwner && config.AlternativePropertiesPrints.ContainsKey(memberInfo.Name))
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

    private static void AddLineToPrint(StringBuilder stringBuilder, int shiftLevel, string name, string value)
    {
        var shift = new string('\t', shiftLevel);
        stringBuilder.Append($"{Environment.NewLine}{shift}{name} = {value}");
    }
}