using System.Globalization;
using ObjectPrinter.PrintingConfigs;

namespace ObjectPrinter.Extensions;

public static class InstancePrintingConfigExtensions
{
	public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this InstancePrintingConfig<TOwner, string> propConfig,
		int maxLen)
	{
		return propConfig.Using(str => str.Length > maxLen ? str[..maxLen] : str);
	}

	public static PrintingConfig<TOwner> Using<TOwner, TType>(this InstancePrintingConfig<TOwner, TType> config,
		CultureInfo culture)
		where TType : IFormattable
	{
		return config.Using(Rule);

		string Rule(TType type) => type.ToString(null, culture);
	}
}