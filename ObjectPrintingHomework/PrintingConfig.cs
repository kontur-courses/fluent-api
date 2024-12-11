using System.Globalization;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrintingHomework;
public class PrintingConfig<TOwner>
{
    private readonly List<Type> excludedTypes = [];
    private readonly List<string> excludedProperties = [];
    private readonly Dictionary<Type, Func<object, string>> typeSerializers = [];
    private readonly Dictionary<string, Func<object, string>> propertySerializers = [];
    private readonly Dictionary<Type, CultureInfo> typeCultures = [];
    private readonly Dictionary<string, Tuple<int, int>> stringPropertyLengths = [];
    private readonly HashSet<object> processedObjects = [];

    public string PrintToString(TOwner obj)
    {
        return PrintToString(obj, 0);
    }

    public PrintingConfig<TOwner> Excluding<TPropType>()
    {
        excludedTypes.Add(typeof(TPropType));
        return this;
    }
    public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        if (memberSelector.Body is not MemberExpression memberExpression)
            throw new ArgumentException("Needed MemberExpression");
        excludedProperties.Add(memberExpression.Member.Name);
        return this;
    }

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this);
    }

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var propertyName = ((MemberExpression)memberSelector.Body).Member.Name;
        return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyName);
    }

    private string PrintToString(object obj, int nestingLevel)
    {
        if (obj == null)
            return string.Empty;

        var type = obj.GetType();

        if (processedObjects.Contains(obj))
            return "Circular Reference";

        if (excludedTypes.Contains(type))
            return string.Empty;
        processedObjects.Add(obj);

        if (typeSerializers.TryGetValue(type, out var serializer))
            return serializer(obj);

        if (typeCultures.TryGetValue(type, out var culture) && obj is IFormattable formattable)
            return formattable.ToString(null, culture);

        if (type.IsSerializable && type.Namespace.StartsWith("System"))
            return obj.ToString();

        var indentation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        sb.AppendLine(type.Name);

        foreach (var propertyInfo in type.GetProperties())
        {
            var propertyType = propertyInfo.PropertyType;
            var propertyName = propertyInfo.Name;
            var propertyValue = propertyInfo.GetValue(obj);

            if (propertySerializers.TryGetValue(propertyName, out var propertySerializer))
            {
                sb.AppendLine(propertySerializer(propertyValue));
                continue;
            }

            if (propertyValue is string stringValue && stringPropertyLengths.TryGetValue(propertyName, out var length))
                propertyValue = stringValue.Substring(Math.Min(length.Item1, stringValue.Length))[..Math.Min(length.Item2, stringValue.Length)];

            if (excludedTypes.Contains(propertyType) || excludedProperties.Contains(propertyName))
                continue;

            sb.Append(indentation + propertyName + " = ");
            sb.AppendLine(PrintToString(propertyValue, nestingLevel + 1));
        }

        return sb.ToString();
    }

    public void AddTypeSerializer<TPropType>(Func<TPropType, string> serializer)
    {
        typeSerializers[typeof(TPropType)] = obj => serializer((TPropType)obj);
    }

    public void AddPropertySerializer(string propertyName, Func<object, string> serializer)
    {
        propertySerializers[propertyName] = serializer;
    }

    public void SetCulture<TPropType>(CultureInfo culture)
    {
        typeCultures[typeof(TPropType)] = culture;
    }

    public void SetStringPropertyLength(string propertyName, int startIndex, int maxLength)
    {
        stringPropertyLengths[propertyName] = new Tuple<int, int>(startIndex, maxLength);
    }
}