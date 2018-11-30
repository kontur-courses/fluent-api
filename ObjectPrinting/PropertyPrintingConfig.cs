using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : Config, 
        IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> config;
        private readonly Expression<Func<TOwner, TPropType>> propertySelector;
        
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => 
            config;
        Expression<Func<TOwner, TPropType>> IPropertyPrintingConfig<TOwner, TPropType>.PropertySelector =>
            propertySelector;

        public PropertyPrintingConfig(PrintingConfig<TOwner> config, 
            Expression<Func<TOwner, TPropType>> propertySelector=null)
        {
            this.config = config;
            this.propertySelector = propertySelector;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> printer)
        {
            config.typePrinters[typeof(TPropType)] = printer as Func<object, string>;
            return config;
        }
    }
}