using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TProperty>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly MemberInfo property;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo property)
        {
            this.printingConfig = printingConfig;
            this.property = property;
        }

        public PrintingConfig<TOwner> SetSerializer(Func<TProperty, string> serializer)
        {
            printingConfig.SetCustomPropertySerializer(property, serializer);
            return printingConfig;
        }
    }
}