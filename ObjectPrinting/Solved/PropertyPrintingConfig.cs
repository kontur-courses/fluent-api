using System;
using System.Globalization;

namespace ObjectPrinting.Solved
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly string propertyName;
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, string propertyName=null)
        {
            this.printingConfig = printingConfig;
            this.propertyName = propertyName;
        }

		public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
		{
		    if (string.IsNullOrWhiteSpace(propertyName))
		        ((IPrintingConfig<TOwner>) printingConfig)
		            .AddPropertySerialisation(propertyName, print as Func<object, string>);
		    else
		        ((IPrintingConfig<TOwner>) printingConfig)
		            .AddTypeSerialisation(typeof(TPropType), print as Func<object, string>);
            return printingConfig;
		}
	}
}