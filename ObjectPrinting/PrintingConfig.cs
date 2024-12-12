using ObjectPrinting;
using System.Globalization;
using System.Linq.Expressions;
using System;
using System.Reflection;


public class PrintingConfig<TOwner>
{
    private readonly PrintingConfigStorage _config = new();

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this);
    }

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var propertyName = ReflectionHelper.GetProperty(memberSelector);
        return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyName);
    }

    public PrintingConfig<TOwner> Excluding<TPropType>()
    {
        _config.ExcludedTypes.Add(typeof(TPropType));
        return this;
    }

    public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var property = ReflectionHelper.GetProperty(memberSelector);
        _config.ExcludedProperties.Add(property);
        return this;
    }

    public void SpecifyTheCulture<TType>(CultureInfo culture)
    {
        _config.TypeCultures[typeof(TType)] = culture;
    }

    public void SpecifyTheCulture(CultureInfo culture, PropertyInfo property)
    {
        _config.PropertyCultures[property] = culture;
    }

    public void AddSerializationMethod<TType>(Func<TType, string> serializationMethod)
    {
        _config.TypeSerializationMethods[typeof(TType)] = obj => serializationMethod((TType)obj);
    }

    public void AddSerializationMethod<TType>(Func<TType, string> serializationMethod, PropertyInfo property)
    {
        _config.PropertySerializationMethods[property] = obj => serializationMethod((TType)obj);
    }

    public void AddLengthProperty(PropertyInfo property, int trimLength)
    {
        _config.PropertyLengths[property] = trimLength;
    }

    public string PrintToString(TOwner obj)
    {
        var serializer = new ObjectSerializer<TOwner>(_config);
        return serializer.Serialize(obj);
    }
}
