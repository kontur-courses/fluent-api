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
    private readonly Dictionary<string, int> stringPropertyLengths = [];

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
        if (memberSelector.Body is MemberExpression memberExpression)
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
            return "null" + Environment.NewLine;
        var type = obj.GetType();

        if (excludedTypes.Contains(type))
            return string.Empty;

        if (typeSerializers.ContainsKey(type))
            return typeSerializers[type](obj) + Environment.NewLine;

        if (typeCultures.TryGetValue(type, out var culture) && obj is IFormattable formattable)
            return formattable.ToString(null, culture) + Environment.NewLine;

        var finalTypes = new[]
        {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
        if (finalTypes.Contains(obj.GetType()))
            return obj + Environment.NewLine;

        var identation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        sb.AppendLine(type.Name);
        foreach (var propertyInfo in type.GetProperties())
        {
            var propertyType = propertyInfo.PropertyType;
            var propertyName = propertyInfo.Name;
            var propertyValue = propertyInfo.GetValue(obj);

            if (propertySerializers.ContainsKey(propertyName))
                return propertySerializers[propertyName](propertyValue) + Environment.NewLine;

            if (propertyValue is string stringValue && stringPropertyLengths.TryGetValue(propertyInfo.Name, out var maxLength))
                propertyValue = stringValue[..Math.Min(maxLength, stringValue.Length)];

            if (excludedTypes.Contains(propertyType) || excludedProperties.Contains(propertyName))
                continue;
            sb.Append(identation + propertyName + " = " +
                      PrintToString(propertyValue,
                          nestingLevel + 1));
        }
        return sb.ToString().Trim();
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

    public void SetStringPropertyLength(string propertyName, int maxLength)
    {
        stringPropertyLengths[propertyName] = maxLength;
    }
}