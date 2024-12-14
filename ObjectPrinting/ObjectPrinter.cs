using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

public class ObjectPrinter
{
    private static readonly HashSet<Type> _finalTypes =
    [
        typeof(Guid),
        typeof(DateTime),
        typeof(TimeSpan),
        typeof(string),
    ];

    public static PrintingConfig<T> For<T>()
    {
        return new PrintingConfig<T>();
    }

    internal static string PrintToString<TOwner>(TOwner? obj, IPrintingConfig<TOwner> printingConfig)
    {
        var sb = new StringBuilder();
        var stack = new Stack<IEnumerator<PropertyValue>>();
        var path = default(PropertyPath);

        if (TryGetStringValue(printingConfig, new PropertyValue(null, obj), stack, ref path, out var stringValue))
        {
            sb.AppendLine(stringValue);
        }

        while (stack.TryPeek(out var enumerator))
        {
            if (!enumerator.MoveNext())
            {
                stack.Pop();
                path = path!.Previous;
                continue;
            }

            var propertyValue = enumerator.Current;
            var indentation = stack.Count;

            if (path != null && path.Contains(propertyValue.Value))
            {
                throw new InvalidOperationException("Unable to print object with cyclic reference.");
            }

            if (TryGetStringValue(printingConfig, propertyValue, stack, ref path, out stringValue))
            {
                sb.Append('\t', indentation);
                sb.AppendLine($"{propertyValue.Name} = {stringValue}");
            }
        }

        return sb.ToString();
    }

    private static bool TryGetStringValue<TOwner>(
        IPrintingConfig<TOwner> printingConfig,
        PropertyValue propertyValue,
        Stack<IEnumerator<PropertyValue>> propertyTreeStack,
        ref PropertyPath? path,
        [MaybeNullWhen(false)] out string stringValue)
    {
        var obj = propertyValue.Value;

        if (obj == null)
        {
            stringValue = "null";
            return true;
        }

        var type = obj.GetType();
        var nextPath = new PropertyPath(propertyValue, path);

        var hasConfig = printingConfig.PropertyConfigsByPath.TryGetValue(nextPath, out var propertyPrintingConfig)
            || printingConfig.PropertyConfigsByType.TryGetValue(type, out propertyPrintingConfig);

        if (hasConfig && propertyPrintingConfig!.IsExcluded)
        {
            stringValue = default;
            return false;
        }

        if (hasConfig && propertyPrintingConfig!.PrintOverride != null)
        {
            stringValue = propertyPrintingConfig.PrintOverride(obj);
        }
        else if (IsFinalType(type))
        {
            var cultureInfo = hasConfig && propertyPrintingConfig!.CultureInfo != null
                ? propertyPrintingConfig.CultureInfo
                : printingConfig.CultureInfo;
            stringValue = PrintFinalTypeToString(obj, cultureInfo);
        }
        else if (obj is IEnumerable enumerable)
        {
            var values = enumerable
                .Cast<object>()
                .Select((item, i) => new PropertyValue($"{{{i}}}", null, item));
            propertyTreeStack.Push(values.GetEnumerator());
            path = nextPath;
            stringValue = GetNameWithoutGenericPart(type);
        }
        else
        {
            if (!printingConfig.IsToLimitNestingLevel
                || propertyTreeStack.Count < printingConfig.MaxNestingLevel)
            {
                propertyTreeStack.Push(GetPropertyValues(obj).GetEnumerator());
                path = nextPath;
            }

            stringValue = GetNameWithoutGenericPart(type);
        }

        return true;
    }

    private static bool IsFinalType(Type type)
    {
        return type.IsPrimitive
            || _finalTypes.Contains(type);
    }

    private static string PrintFinalTypeToString(object obj, CultureInfo cultureInfo)
    {
        return obj is IFormattable formattableObj
            ? formattableObj.ToString(null, cultureInfo)
            : obj.ToString()!;
    }

    private static IEnumerable<PropertyValue> GetPropertyValues(object obj)
    {
        return obj
            .GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.GetIndexParameters().Length == 0)
            .Select(property => new PropertyValue(property, property.GetValue(obj)));
    }

    private static string GetNameWithoutGenericPart(Type type)
    {
        var end = type.Name.IndexOf('`');
        return end == -1
            ? type.Name
            : type.Name[..end];
    }
}
