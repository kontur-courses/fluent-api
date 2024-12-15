using System.Collections;
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
        var path = new PropertyPath(new PropertyValue(null, obj));

        if (!IsExcluded(path, printingConfig, 0))
        {
            var sb = new StringBuilder();
            AppendPrintedObject(sb, path, printingConfig, 0);
            return sb.ToString();
        }

        return string.Empty;
    }

    private static void AppendPrintedObject<TOwner>(
        StringBuilder sb,
        PropertyPath path,
        IPrintingConfig<TOwner> printingConfig,
        int nestingLevel)
    {
        var obj = path.PropertyValue.Value;

        if (obj == null)
        {
            sb.AppendLine("null");
        }
        else if (IsCyclicReference(path))
        {
            AppendCyclicReference(sb, path);
        }
        else if (IsAlternativelyPrintedObject(path, printingConfig))
        {
            AppendAlternativelyPrintedObject(sb, path, printingConfig);
        }
        else if (IsFinalTypeObject(obj))
        {
            AppendFinalTypeObject(sb, path, printingConfig);
        }
        else if (obj is IEnumerable)
        {
            AppendEnumerableObjectWithItems(sb, path, printingConfig, nestingLevel);
        }
        else
        {
            AppendObjectWithProperties(sb, path, printingConfig, nestingLevel);
        }
    }

    private static bool IsCyclicReference(PropertyPath path)
    {
        var obj = path.PropertyValue.Value!;
        return path.Previous != null
            && path.Previous.Contains(obj);
    }

    private static void AppendCyclicReference(StringBuilder sb, PropertyPath path)
    {
        var obj = path.PropertyValue.Value!;
        var pathToExistingValue = path.Previous!.FindPathTo(obj);
        sb.AppendLine($"[root{pathToExistingValue}]");
    }

    private static bool IsAlternativelyPrintedObject<TOwner>(PropertyPath path, IPrintingConfig<TOwner> printingConfig)
    {
        return printingConfig.TryGetConfig(path, out var propertyPrintingConfig)
            && propertyPrintingConfig.PrintOverride != null;
    }

    private static void AppendAlternativelyPrintedObject<TOwner>(
        StringBuilder sb,
        PropertyPath path,
        IPrintingConfig<TOwner> printingConfig)
    {
        if (printingConfig.TryGetConfig(path, out var propertyPrintingConfig))
        {
            var obj = path.PropertyValue.Value!;
            var stringValue = propertyPrintingConfig.PrintOverride!(obj);
            sb.AppendLine(stringValue);
        }
    }

    private static bool IsFinalTypeObject(object obj)
    {
        var type = obj.GetType();
        return type.IsPrimitive
            || _finalTypes.Contains(type);
    }

    private static void AppendFinalTypeObject<TOwner>(
        StringBuilder sb,
        PropertyPath path,
        IPrintingConfig<TOwner> printingConfig)
    {
        var cultureInfo = printingConfig.TryGetConfig(path, out var propertyPrintingConfig)
            && propertyPrintingConfig.CultureInfo != null
            ? propertyPrintingConfig.CultureInfo
            : printingConfig.CultureInfo;

        var obj = path.PropertyValue.Value!;
        var stringValue = obj is IFormattable formattableObj
            ? formattableObj.ToString(null, cultureInfo)
            : obj.ToString();

        sb.AppendLine(stringValue);
    }

    private static void AppendEnumerableObjectWithItems<TOwner>(
        StringBuilder sb,
        PropertyPath path,
        IPrintingConfig<TOwner> printingConfig,
        int nestingLevel)
    {
        var enumerable = (IEnumerable)path.PropertyValue.Value!;
        var type = enumerable.GetType();
        var typeName = GetTypeNameWithoutGenericPart(type);
        var values = enumerable
            .Cast<object>()
            .Select((item, i) => new PropertyValue($"{{{i}}}", type, item));

        sb.AppendLine(typeName);
        AppendPropertyValues(sb, path, printingConfig, values, nestingLevel);
    }

    private static void AppendObjectWithProperties<TOwner>(
        StringBuilder sb,
        PropertyPath path,
        IPrintingConfig<TOwner> printingConfig,
        int nestingLevel)
    {
        var obj = path.PropertyValue.Value!;
        var type = obj.GetType();
        var typeName = GetTypeNameWithoutGenericPart(type);
        var values = type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.GetIndexParameters().Length == 0)
            .Select(property => new PropertyValue(property, property.GetValue(obj)));

        sb.AppendLine(typeName);
        AppendPropertyValues(sb, path, printingConfig, values, nestingLevel);
    }

    private static void AppendPropertyValues<TOwner>(
        StringBuilder sb,
        PropertyPath path,
        IPrintingConfig<TOwner> printingConfig,
        IEnumerable<PropertyValue> propertyValues,
        int nestingLevel)
    {
        foreach (var propertyValue in propertyValues)
        {
            var nextPath = new PropertyPath(propertyValue, path);

            if (!IsExcluded(nextPath, printingConfig, nestingLevel + 1))
            {
                sb.Append('\t', nestingLevel + 1);
                sb.Append(propertyValue.Name);
                sb.Append(" = ");
                AppendPrintedObject(sb, nextPath, printingConfig, nestingLevel + 1);
            }
        }
    }

    private static bool IsExcluded<TOwner>(PropertyPath path, IPrintingConfig<TOwner> printingConfig, int nestingLevel)
    {
        return printingConfig.IsToLimitNestingLevel
            && nestingLevel > printingConfig.MaxNestingLevel
            || printingConfig.TryGetConfig(path, out var propertyPrintingConfig)
                && propertyPrintingConfig!.IsExcluded;
    }

    private static string GetTypeNameWithoutGenericPart(Type type)
    {
        var end = type.Name.IndexOf('`');
        return end == -1
            ? type.Name
            : type.Name[..end];
    }
}
