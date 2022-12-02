namespace ObjectPrinter.PrintingConfigs;

public abstract class InstancePrintingConfig<TOwner, TPropType>
{
	protected readonly PrintingConfig<TOwner> PrintingConfig;

	protected InstancePrintingConfig(PrintingConfig<TOwner> printingConfig)
	{
		PrintingConfig = printingConfig;
	}

	public abstract PrintingConfig<TOwner> Using(Func<TPropType, string> printingRule);
}