using System;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly Expression<Func<TOwner, TPropType>> memberSelector;

        public PropertyPrintingConfig(PrintingConfig<TOwner> _printingConfig)
        {
            printingConfig = _printingConfig;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> _printingConfig, Expression<Func<TOwner, TPropType>> _memberSelector)
        {
            printingConfig = _printingConfig;
            memberSelector = _memberSelector;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (memberSelector == null)
                printingConfig.SetTypeSerialization(print);
            else
                printingConfig.SetMemberSerialization(memberSelector, print);
            return printingConfig;
        }
                
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}
