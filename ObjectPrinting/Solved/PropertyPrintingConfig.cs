using System;

namespace ObjectPrinting.Solved
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly string propertyName;
        string IPropertyPrintingConfig<TOwner, TPropType>.PropName => propertyName;
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, string propertyName=null)
        {
            this.printingConfig = printingConfig;
            this.propertyName = propertyName;
        }

		public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
		{
		    if (!string.IsNullOrWhiteSpace(propertyName))
		        ((IPrintingConfig<TOwner>) printingConfig)
		            .AddPropertySerialisation(propertyName, obj => print((TPropType)obj));
		    else
		        ((IPrintingConfig<TOwner>) printingConfig)
		            .AddTypeSerialisation(typeof(TPropType), obj => print((TPropType)obj));
            return printingConfig;
		}
	}
}