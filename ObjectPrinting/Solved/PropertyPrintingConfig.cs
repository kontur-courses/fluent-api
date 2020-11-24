using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Linq;

namespace ObjectPrinting.Solved
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        internal readonly string fullNameProp;
        private readonly PrintingConfig<TOwner> printingConfig;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, Expression<Func<TOwner, TPropType>> expression = null)
        {
            this.printingConfig = printingConfig;
            fullNameProp = expression?.GetFullNameProperty();
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if(fullNameProp == null)
                printingConfig.AddSerialization(print);
            else
                printingConfig.AddSerialization(fullNameProp, print);
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}