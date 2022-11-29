using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    internal readonly Dictionary<PropertyInfo, Func<object, string>> AlternativePrintingsForProperties = new();
    internal readonly Dictionary<Type, Func<object, string>> AlternativePrintingsForTypes = new();

    private readonly List<PropertyInfo> excludedProperties = new();
    private readonly List<Type> excludedTypes = new();

    private readonly Type[] finalTypes =
    {
        typeof(int), typeof(double), typeof(float), typeof(string),
        typeof(DateTime), typeof(TimeSpan)
    };

    internal readonly Dictionary<Type, CultureInfo> TypesCultures = new();
    internal int? StringPropertyTrimIndex = null;

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this);
    }

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
        Expression<Func<TOwner, TPropType>> memberSelector)
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this, GetPropertyInfo(memberSelector));
    }

    public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        excludedProperties.Add(GetPropertyInfo(memberSelector));

        return this;
    }

    private static PropertyInfo GetPropertyInfo<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var body = memberSelector.Body;

        if (!IsPropertyCall(body))
            throw new ArgumentException($"Member selector ({memberSelector}) isn't property call");

        return ((body as MemberExpression)!.Member as PropertyInfo)!;
    }

    private static bool IsPropertyCall(Expression expression)
    {
        var isMemberCall = expression.NodeType == ExpressionType.MemberAccess;
        if (!isMemberCall) return false;

        var isPropertyCall = (expression as MemberExpression)!.Member.MemberType == MemberTypes.Property;
        return isPropertyCall;
    }

    public PrintingConfig<TOwner> Excluding<TPropType>()
    {
        excludedTypes.Add(typeof(TPropType));
        return this;
    }

    public string PrintToString(TOwner obj)
    {
        return PrintToStringLogic(obj);
    }

    private string PrintToStringLogic(object? obj)
    {
        if (obj is null)
            return "null";

        var type = obj.GetType();

        if (finalTypes.Contains(type))
        {
            if (TypesCultures.ContainsKey(type) && obj is IConvertible convertible)
                return convertible.ToString(TypesCultures[type]);

            var result = obj.ToString();
            if (StringPropertyTrimIndex.HasValue)
                result = result[..Math.Min(StringPropertyTrimIndex.Value, result.Length)];
            return result;
        }

        var sb = new StringBuilder();
        sb.Append(type.Name);
        var newLine = Environment.NewLine;
        foreach (var property in type.GetProperties().Where(NotExcluded))
        {
            var print = GetPrint(property)(property.GetValue(obj)).Replace(newLine, $"{newLine}\t");
            sb.Append($"{newLine}\t{property.Name} = {print}");
        }

        return sb.ToString();
    }

    private Func<object, string> GetPrint(PropertyInfo property)
    {
        if (AlternativePrintingsForProperties.ContainsKey(property))
            return obj => AlternativePrintingsForProperties[property](obj);

        if (AlternativePrintingsForTypes.ContainsKey(property.PropertyType))
            return obj => AlternativePrintingsForTypes[property.PropertyType](obj);

        return PrintToStringLogic;
    }

    private bool NotExcluded(PropertyInfo property)
    {
        return !excludedProperties.Contains(property) && !excludedTypes.Contains(property.PropertyType);
    }
}