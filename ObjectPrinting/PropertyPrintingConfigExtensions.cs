using System;

namespace ObjectPrinting;

public static class PropertyPrintingConfigExtensions
{
    public static PrintingConfig<TOwner> SetSerializationStyle<TOwner, TPropType>
        (this PropertyPrintingConfig<TOwner, TPropType> propertyConfig, PrintingConfig<TOwner>.SerializerDelegate serializer)
    {
        var accessor = (IPropertyPrintingConfigAccessor<TOwner, TPropType>)propertyConfig;
        var propertyName = PropertyPath.ConvertFromSelector(accessor.PropertySelector!);
        return accessor.ParentConfig.WithPropertySerialization(propertyName, serializer);
    }

    public static PrintingConfig<TOwner> TrimStringToLength<TOwner>
        (this PropertyPrintingConfig<TOwner, string> propertyConfig, int length)
    {
        var accessor = (IPropertyPrintingConfigAccessor<TOwner, string>)propertyConfig;
        return accessor.ParentConfig.TrimStringToLength(accessor.PropertySelector!, length);
    }

    public static PrintingConfig<TOwner> Exclude<TOwner, TPropType>
        (this PropertyPrintingConfig<TOwner, TPropType> propertyConfig)
    {
        var accessor = (IPropertyPrintingConfigAccessor<TOwner, TPropType>)propertyConfig;
        var propertyName = PropertyPath.ConvertFromSelector(accessor.PropertySelector!);
        return accessor.ParentConfig.Exclude(propertyName);
    }
}