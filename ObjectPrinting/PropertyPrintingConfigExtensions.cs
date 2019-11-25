using System;
using System.Globalization;
using System.Linq;

namespace ObjectPrinting
{
	public static class PropertyPrintingConfigExtensions
	{
		public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
		{
			return config(ObjectPrinter.For<T>()).PrintToString(obj);
		}

		public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
			this PropertyPrintingConfig<TOwner, string> propertyConfig, int maxLength)
		{
			var propConfig = (IPropertyPrintingConfig<TOwner, string>) propertyConfig;
			var printingConfig = (IPrintingConfig<TOwner>) propConfig.ParentConfig;
			var propName = printingConfig.GetPropertyName(propConfig.Member);
			
			printingConfig.PropertiesPrintingBehaviors.Add(propName,
				obj => string.Join("", ((string) obj).Take(maxLength)));
			return propConfig.ParentConfig;
		}
		
		public static PrintingConfig<TOwner> Using<TOwner>(
			this PropertyPrintingConfig<TOwner, double> propertyConfig, CultureInfo culture)
		{
			var printingConfig = ((IPropertyPrintingConfig<TOwner, double>) propertyConfig).ParentConfig;
			var printingBehaviors = ((IPrintingConfig<TOwner>) printingConfig).TypesPrintingBehaviors;
			printingBehaviors.Add(typeof(double), obj => ((double) obj).ToString(culture));
			return printingConfig;
		}
	}
}