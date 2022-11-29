using System.Collections;
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
        return PrintToString(obj, new List<object>());
    }

    private string PrintToString(object? obj, IEnumerable<object> printedObjects)
    {
        if (obj is null) return "null";

        var type = obj.GetType();

        if (AlternativePrintingsForTypes.ContainsKey(type))
            return AlternativePrintingsForTypes[type](obj);

        if (finalTypes.Contains(type))
            return PrintFinalType(obj);

        var printedObjectsArray = printedObjects.ToArray();
        if (!type.IsValueType && printedObjectsArray.Contains(obj))
            throw new InvalidOperationException("Printable object contains circular reference");

        printedObjects = printedObjectsArray.Append(obj);
        return obj is ICollection collection
            ? PrintCollection(collection, printedObjects)
            : PrintComplexObject(obj, printedObjects);
    }

    private bool NotExcluded(PropertyInfo property)
    {
        return !excludedProperties.Contains(property) && !excludedTypes.Contains(property.PropertyType);
    }

    private string PrintFinalType(object obj)
    {
        var type = obj.GetType();
        
        if (StringPropertyTrimIndex.HasValue && obj is string str)
            obj = str[..Math.Min(StringPropertyTrimIndex.Value, str.Length)];

        if (TypesCultures.ContainsKey(type) && obj is IConvertible convertible)
            return convertible.ToString(TypesCultures[type]);

        return obj.ToString();
    }

    private string PrintCollection(ICollection collection, IEnumerable<object> printedObjects)
    {
        var newLine = Environment.NewLine;
        
        var sb = new StringBuilder(collection.GetType().Name + newLine + '\t');

        var itemsStrings = collection
            .Cast<object>()
            .Select(o => PrintToString(o, printedObjects).Replace(newLine, newLine + '\t'));

        var allFinal = collection.Cast<object>().All(o => finalTypes.Contains(o.GetType()));
            
        sb.Append($"[{string.Join(allFinal ? ", " : $",{newLine}\t", itemsStrings)}]");

        return sb.ToString();
    }
    
    private string PrintComplexObject(object obj, IEnumerable<object> printedObjects){
        var newLine = Environment.NewLine;

        var type = obj.GetType();
        
        var sb = new StringBuilder(type.Name);
        
        foreach (var property in type.GetProperties().Where(NotExcluded))
        {
            var result = (AlternativePrintingsForProperties.ContainsKey(property)
                    ? AlternativePrintingsForProperties[property](property.GetValue(obj))
                    : PrintToString(property.GetValue(obj), printedObjects))
                .Replace(newLine, $"{newLine}\t");

            sb.Append($"{newLine}\t{property.Name} = {result}");
        }

        return sb.ToString();
    }
}