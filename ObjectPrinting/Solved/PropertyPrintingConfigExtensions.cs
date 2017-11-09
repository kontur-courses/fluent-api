using System;

namespace ObjectPrinting.Solved
{
	public static class PropertyPrintingConfigExtensions
	{
		public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
		{
			return config(ObjectPrinter.For<T>()).PrintToString(obj);
		}

		public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
		{
			return ((IPropertyPrintingConfig<TOwner, string>)propConfig).ParentConfig;
		}

		public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> propConfig, CultureInfo cultureInfo)
		{
			return ((IPropertyPrintingConfig<TOwner, int>)propConfig).ParentConfig;
		}

		public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, long> propConfig, CultureInfo cultureInfo)
		{
			return ((IPropertyPrintingConfig<TOwner, long>)propConfig).ParentConfig;
		}

		public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> propConfig, CultureInfo cultureInfo)
		{
			return ((IPropertyPrintingConfig<TOwner, double>)propConfig).ParentConfig;
		}

		public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, float> propConfig, CultureInfo cultureInfo)
		{
			return ((IPropertyPrintingConfig<TOwner, float>)propConfig).ParentConfig;
		}
	}
}