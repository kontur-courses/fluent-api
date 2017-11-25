using System;
using System.Globalization;

namespace ObjectPrinting.Solved
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
		{
			this.printingConfig = printingConfig;
		}

		public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
		{
			return printingConfig;
		}
	}
}