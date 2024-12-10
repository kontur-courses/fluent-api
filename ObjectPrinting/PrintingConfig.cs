using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    internal PrintingConfig()
    {
    }

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this);
    }

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
        Expression<Func<TOwner, TPropType>> propertySelector)
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this);
    }

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
        Func<PropertyInfo, bool> propertyFilter)
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this);
    }

    public PrintingConfig<TOwner> Excluding<TPropType>()
    {
        return this;
    }

    public PrintingConfig<TOwner> Excluding<TPropType>(
        Expression<Func<TOwner, TPropType>> propertySelector)
    {
        return this;
    }

    public PrintingConfig<TOwner> Excluding(
        Func<PropertyInfo, bool> propertyFilter)
    {
        return this;
    }

    public PrintingConfig<TOwner> Using(CultureInfo culture)
    {
        return this;
    }

    public string PrintToString(TOwner obj)
    {
        throw new NotImplementedException();
    }
}
