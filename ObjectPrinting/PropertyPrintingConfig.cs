using System;
using System.Globalization;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropType>
{
	private readonly PrintingConfig<TOwner> parentConfig;
	private readonly string? propertyName;

	internal PrintingConfig<TOwner> ParentConfig => parentConfig;
	internal string? PropertyName => propertyName;

	public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig, string? propertyName)
	{
		this.parentConfig = parentConfig;
		this.propertyName = propertyName;
	}

	public PropertyPrintingConfig(PrintingConfig<TOwner> parentConfig) =>
		this.parentConfig = parentConfig;

	public PrintingConfig<TOwner> Using(Func<TPropType, string> serialize)
	{
		if (string.IsNullOrEmpty(propertyName))
			parentConfig.AddTypeSerializer(serialize);
		else
			parentConfig.AddPropertySerializer(propertyName, serialize);

		return parentConfig;
	}
}