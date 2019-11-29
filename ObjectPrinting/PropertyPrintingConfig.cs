using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly MemberInfo propertyInfo;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo propertyInfo)
        {
            this.printingConfig = printingConfig;
            this.propertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            var objPrint = FuncСonverter.CastToObjectPrint(print);
            if (propertyInfo == null)
            {
                printingConfig.AddTypeAltPrinting(typeof(TPropType), objPrint);
            }
            else
            {
                printingConfig.AddPropAltPrinting(propertyInfo, objPrint);
            }
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}