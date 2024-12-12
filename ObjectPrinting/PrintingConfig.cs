using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting;

public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
{
    private readonly Dictionary<Type, IPropertyPrintingConfig<TOwner>> _propertyConfigsByType = [];
    private readonly Dictionary<PropertyPath, IPropertyPrintingConfig<TOwner>> _propertyConfigsByPath = [];

    private CultureInfo _cultureInfo = CultureInfo.InvariantCulture;
    private bool _isToLimitNestingLevel = false;
    private int _maxNestingLevel = 10;

    internal PrintingConfig()
    {
    }

    Dictionary<Type, IPropertyPrintingConfig<TOwner>> IPrintingConfig<TOwner>.PropertyConfigsByType
        => _propertyConfigsByType;

    Dictionary<PropertyPath, IPropertyPrintingConfig<TOwner>> IPrintingConfig<TOwner>.PropertyConfigsByPath
        => _propertyConfigsByPath;

    CultureInfo IPrintingConfig<TOwner>.CultureInfo => _cultureInfo;

    bool IPrintingConfig<TOwner>.IsToLimitNestingLevel => _isToLimitNestingLevel;

    int IPrintingConfig<TOwner>.MaxNestingLevel => _maxNestingLevel;

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
    {
        var propertyType = typeof(TPropType);

        if (!_propertyConfigsByType.TryGetValue(propertyType, out var propertyPrintingConfig))
        {
            propertyPrintingConfig = new PropertyPrintingConfig<TOwner, TPropType>(this);
            _propertyConfigsByType[propertyType] = propertyPrintingConfig;
        }

        return (PropertyPrintingConfig<TOwner, TPropType>)propertyPrintingConfig;
    }

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
        Expression<Func<TOwner, TPropType>> propertySelector)
    {
        ArgumentNullException.ThrowIfNull(propertySelector);

        if (!PropertyPath.TryGetPropertyPath(propertySelector, out var propertyPath))
        {
            var propertySelectorString = propertySelector.Body.ToString();
            throw new ArgumentException($"Expression '{propertySelector}' is not a property path.");
        }

        if (!_propertyConfigsByPath.TryGetValue(propertyPath, out var propertyPrintingConfig))
        {
            propertyPrintingConfig = new PropertyPrintingConfig<TOwner, TPropType>(this);
            _propertyConfigsByPath[propertyPath] = propertyPrintingConfig;
        }

        return (PropertyPrintingConfig<TOwner, TPropType>)propertyPrintingConfig;
    }

    public PrintingConfig<TOwner> Excluding<TPropType>()
    {
        return Printing<TPropType>().Exclude();
    }

    public PrintingConfig<TOwner> Excluding<TPropType>(
        Expression<Func<TOwner, TPropType>> propertySelector)
    {
        ArgumentNullException.ThrowIfNull(propertySelector);

        return Printing(propertySelector).Exclude();
    }

    public PrintingConfig<TOwner> UsingCulture(CultureInfo cultureInfo)
    {
        ArgumentNullException.ThrowIfNull(cultureInfo);

        _cultureInfo = cultureInfo;
        return this;
    }

    public PrintingConfig<TOwner> UsingMaxNestingLevel(int maxNestingLevel)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxNestingLevel);

        _isToLimitNestingLevel = true;
        _maxNestingLevel = maxNestingLevel;
        return this;
    }

    public string PrintToString(TOwner? obj)
    {
        return ObjectPrinter.PrintToString(obj, this);
    }
}
