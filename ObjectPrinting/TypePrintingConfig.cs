using System;

namespace ObjectPrinting;

public class TypePrintingConfig<TOwner, TPropType>
{
	private readonly PrintingConfig<TOwner> parentConfig;

	public TypePrintingConfig(PrintingConfig<TOwner> parentConfig) =>
		this.parentConfig = parentConfig;

	public PrintingConfig<TOwner> Using(Func<TPropType, string> serialize)
	{
		parentConfig.AddTypeSerializer(serialize);

		return parentConfig;
	}
}