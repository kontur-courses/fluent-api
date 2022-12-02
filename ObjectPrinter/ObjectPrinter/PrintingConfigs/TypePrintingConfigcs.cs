namespace ObjectPrinter.PrintingConfigs;

public class TypePrintingConfig<TOwner, TType> : InstancePrintingConfig<TOwner, TType>
{
	public TypePrintingConfig(PrintingConfig<TOwner> printingConfig) : base(printingConfig)
	{
	}

	public override PrintingConfig<TOwner> Using(Func<TType, string> printRule)
	{
		PrintingConfig.TypesPrintingRules.Add(typeof(TType), printRule);
		return PrintingConfig;
	}
}