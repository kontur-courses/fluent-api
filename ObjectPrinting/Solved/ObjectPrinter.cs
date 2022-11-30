using System;
using System.Collections.Generic;
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

    public static string PrintToString<TOwner, TCollection>(this TCollection collectionOfObjects,
        PrintingConfig<TOwner> config)
        where TCollection : IEnumerable<TOwner>
    {
        var stringBuilder = new StringBuilder();
        foreach (var obj in collectionOfObjects)
        {
            var printedObject = PrintToString(obj, config);
            stringBuilder.Append($"{printedObject}{Environment.NewLine}");
        }

        return stringBuilder.ToString();
    }

    public static string PrintToString<TOwner, TKey, TValue>(this IDictionary<TKey, TValue> collectionOfObjects,
        PrintingConfig<TOwner> config)
    {
        var stringBuilder = new StringBuilder();
        foreach (var obj in collectionOfObjects)
        {
            var printedKey = obj.Key.ToString();
            var printedValue = obj.Value.ToString();
            if (obj.Key is TOwner key)
                printedKey = PrintToString(key, config);
            if (obj.Value is TOwner value)
                printedValue = PrintToString(value, config);
            stringBuilder.Append($"{printedKey} - {{{printedValue}{Environment.NewLine}}}{Environment.NewLine}");
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

        var stringBuilder = new StringBuilder();
        foreach (var memberInfo in config.Members)
        {
            var memberType = MemberInfoHelper.GetTypeFromMemberInfo(memberInfo);
            var memberValue = MemberInfoHelper.GetValueFromMemberInfo(obj, memberInfo);

            if (memberType.IsValueType == false && memberType != typeof(string) &&
                memberValue is not null && printedObjects.Contains(memberValue))
                continue;
            printedObjects.Add(memberValue);

            string printedMember;
            if (config.AlternativePropertiesPrints.ContainsKey(memberInfo.Name))
            {
                printedMember = config.AlternativePropertiesPrints[memberInfo.Name](memberValue);
            }
            else if (config.AlternativeTypesPrints.ContainsKey(memberType))
            {
                printedMember = config.AlternativeTypesPrints[memberType](memberValue);
            }
            else
            {
                if (memberValue is TOwner)
                    printedMember = PrintToString(memberValue, config, nestingLevel + 1,
                        new HashSet<object>(printedObjects));
                else
                    printedMember = PrintToString(memberValue, memberValue?.GetDefaultPrintingConfig(),
                        nestingLevel + 1,
                        new HashSet<object>(printedObjects));
            }

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