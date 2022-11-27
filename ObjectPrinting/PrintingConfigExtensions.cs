using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

public static class PrintingConfigExtensions
{
    public static PrintingConfig<TOwner> WithCyclicInheritanceHandler<TOwner>(
        this PrintingConfig<TOwner> printingConfig, CyclicInheritanceHandler cyclicInheritanceHandler)
    {
        ((IInternalPrintingConfig<TOwner>)printingConfig).GetRoot().CyclicInheritanceHandler =
            cyclicInheritanceHandler;
        return printingConfig;
    }

    public static void ConsumeProperty<TOwner>(this RootPrintingConfig<TOwner> printingConfig, object obj,
        int nestingLevel,
        StringBuilder stringBuilder, PropertyInfo propertyInfo,
        char indentation, HashSet<object> cyclicInheritanceIgnoredObjects)
    {
        if (printingConfig.TypeExcluding.Contains(propertyInfo.PropertyType) ||
            printingConfig.MemberExcluding.Contains(propertyInfo))
            return;

        stringBuilder.Append(indentation, nestingLevel)
            .Append(propertyInfo.Name).Append(" = ");
        var value = propertyInfo.GetValue(obj);
        printingConfig.GetPropertyStringValue(nestingLevel, value, propertyInfo, stringBuilder,
            cyclicInheritanceIgnoredObjects);
    }

    public static void ConsumeItems<TOwner>(this RootPrintingConfig<TOwner> printingConfig, IEnumerable source,
        int nestingLevel, StringBuilder stringBuilder, char indentation,
        HashSet<object> cyclicInheritanceIgnoredObjects)
    {
        stringBuilder.Append(indentation, nestingLevel)
            .AppendLine("Items");
        var counter = 0;
        foreach (var item in source)
        {
            var itemType = item.GetType();
            if (itemType.GetProperty("Key", BindingFlags.Public | BindingFlags.Instance) is { } keyInfo &&
                itemType.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance) is { } valueInfo)
            {
                stringBuilder.Append(indentation, nestingLevel + 1)
                    .Append('[');
                printingConfig.GetPropertyStringValue(nestingLevel + 1, keyInfo.GetValue(item), keyInfo, stringBuilder,
                    cyclicInheritanceIgnoredObjects, false);
                stringBuilder.Append("] = ");
                var value = valueInfo.GetValue(item);

                printingConfig.GetPropertyStringValue(nestingLevel + 1, valueInfo.GetValue(item), valueInfo,
                    stringBuilder,
                    cyclicInheritanceIgnoredObjects);
            }
            else
            {
                stringBuilder.Append(indentation, nestingLevel + 1)
                    .Append('[')
                    .Append(counter++)
                    .Append("] = ");

                GetItemStringValue(printingConfig, nestingLevel, stringBuilder, cyclicInheritanceIgnoredObjects, item,
                    itemType);
            }
        }
    }

    private static void GetItemStringValue<TOwner>(RootPrintingConfig<TOwner> printingConfig, int nestingLevel,
        StringBuilder stringBuilder, HashSet<object> cyclicInheritanceIgnoredObjects, object item, Type itemType)
    {
        if (TryReturnNull(item, out var propertyStringValue) ||
            printingConfig.TryReturnTypeStringValue(itemType, item, out propertyStringValue) ||
            printingConfig.TryReturnAssignableTypeStringValue(itemType, item,
                out propertyStringValue))
            stringBuilder.AppendLine(propertyStringValue);
        else
            printingConfig.PrintToString(item, nestingLevel + 1, stringBuilder, cyclicInheritanceIgnoredObjects);
    }

    public static void ConsumeField<TOwner>(this RootPrintingConfig<TOwner> printingConfig, object obj,
        int nestingLevel,
        StringBuilder stringBuilder, FieldInfo fieldInfo,
        char indentation, HashSet<object> cyclicInheritanceIgnoredObjects)
    {
        if (printingConfig.TypeExcluding.Contains(fieldInfo.FieldType) ||
            printingConfig.MemberExcluding.Contains(fieldInfo))
            return;

        stringBuilder.Append(indentation, nestingLevel).Append(fieldInfo.Name).Append(" = ");
        var value = fieldInfo.GetValue(obj);
        printingConfig.GetFieldStringValue(nestingLevel + 1, value, fieldInfo, stringBuilder,
            cyclicInheritanceIgnoredObjects);
    }


    private static void GetFieldStringValue<T>(this RootPrintingConfig<T> printingConfig, int nestingLevel,
        object? value,
        FieldInfo fieldInfo,
        StringBuilder stringBuilder, HashSet<object> cyclicInheritanceIgnoredObjects)
    {
        if (TryReturnNull(value, out var fieldStringValue) ||
            printingConfig.TryReturnMemberStringValue(fieldInfo, value!, out fieldStringValue) ||
            printingConfig.TryReturnTypeStringValue(fieldInfo.FieldType, value!, out fieldStringValue) ||
            printingConfig.TryReturnAssignableTypeStringValue(fieldInfo.FieldType, value!, out fieldStringValue))
            stringBuilder.AppendLine(fieldStringValue);
        else
            printingConfig.PrintToString(value!, nestingLevel, stringBuilder, cyclicInheritanceIgnoredObjects);
    }


    private static void GetPropertyStringValue<T>(this RootPrintingConfig<T> printingConfig, int nestingLevel,
        object? value,
        PropertyInfo propertyInfo,
        StringBuilder stringBuilder, HashSet<object> cyclicInheritanceIgnoredObjects, bool needNewLine = true)
    {
        if (TryReturnNull(value, out var propertyStringValue) ||
            printingConfig.TryReturnMemberStringValue(propertyInfo, value!, out propertyStringValue) ||
            printingConfig.TryReturnTypeStringValue(propertyInfo.PropertyType, value!, out propertyStringValue) ||
            printingConfig.TryReturnAssignableTypeStringValue(propertyInfo.PropertyType, value!,
                out propertyStringValue))
        {
            stringBuilder.Append(propertyStringValue);
            if (needNewLine)
                stringBuilder.AppendLine();
        }
        else
        {
            printingConfig.PrintToString(value!, nestingLevel, stringBuilder, cyclicInheritanceIgnoredObjects);
        }
    }


    private static bool TryReturnAssignableTypeStringValue(this IInternalPrintingConfigStorage printingConfig,
        Type type,
        object value, out string? stringValue)
    {
        stringValue = null;

        if (printingConfig.IgnoredTypesFromAssignableCheck.Contains(type) ||
            printingConfig.TypeSerializers.ContainsKey(type))
            return false;
        var (_, assignableSerializer) =
            printingConfig.AssignableTypeSerializers.FirstOrDefault(x => type.IsAssignableTo(x.Type));
        if (assignableSerializer == null)
        {
            printingConfig.IgnoredTypesFromAssignableCheck.Add(type);
            return false;
        }

        printingConfig.TypeSerializers[type] = assignableSerializer;
        return printingConfig.TryReturnTypeStringValue(type, value, out stringValue);
    }


    private static bool TryReturnTypeStringValue(this IInternalPrintingConfigStorage printingConfig, Type type,
        object value,
        out string? stringValue)
    {
        stringValue = null;
        if (!printingConfig.TypeSerializers.TryGetValue(type, out var serializer)) return false;
        stringValue = string.Concat(serializer(value));
        return true;
    }

    private static bool TryReturnMemberStringValue(this IInternalPrintingConfigStorage printingConfig,
        MemberInfo memberInfo,
        object value, out string? stringValue)
    {
        stringValue = null;
        if (!printingConfig.MemberSerializers.TryGetValue(memberInfo, out var serializer)) return false;
        stringValue = string.Concat(serializer(value));
        return true;
    }

    public static bool TryReturnNull(object? value, out string? stringValue)
    {
        stringValue = null;
        if (value is not null) return false;
        stringValue = "null";
        return true;
    }
}