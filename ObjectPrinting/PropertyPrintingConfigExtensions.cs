using System;

namespace ObjectPrinting;

public static class PropertyPrintingConfigExtensions
{
	public static PrintingConfig<TOwner> Trim<TOwner>(this PropertyPrintingConfig<TOwner, string> propertyPrintingConfig, int length)
	{
		ArgumentNullException.ThrowIfNull(propertyPrintingConfig.PropertyMemberInfo);
		propertyPrintingConfig.ParentConfig.AddStringPropertyTrim(propertyPrintingConfig.PropertyMemberInfo, length);

		return propertyPrintingConfig.ParentConfig;
	}
}