using System.Reflection;

namespace ObjectPrinter.PrintingConfigs;

public class PropertyPrintingConfig<TOwner, TPropType> : InstancePrintingConfig<TOwner, TPropType>
{
	private readonly MemberInfo member;

	public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo member) : base(printingConfig)
	{
		this.member = member;
	}

	public override PrintingConfig<TOwner> Using(Func<TPropType, string> printingRule)
	{
		PrintingConfig.MembersPrintingRules.Add(member, printingRule);
		return PrintingConfig;
	}
}