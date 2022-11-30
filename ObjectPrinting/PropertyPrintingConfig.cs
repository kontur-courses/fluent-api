using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner,TType> : IPropertyPrintingConfig<TOwner,TType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly SerializingOptions serializingOptions;
        private readonly MemberInfo memberInfo;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig,SerializingOptions serializingOptions,MemberInfo memberInfo)
        {
            this.printingConfig = printingConfig;
            this.serializingOptions = serializingOptions;
            this.memberInfo = memberInfo;
        }
        
        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig,SerializingOptions serializingOptions)
        {
            this.printingConfig = printingConfig;
            this.serializingOptions = serializingOptions;
        }

        public PrintingConfig<TOwner> Using(Func<TType, string> rule) 
        {
            if (rule is null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            if (memberInfo is null)
            {
                serializingOptions.AddSerializationRule(rule);
            }
            else
            {
                serializingOptions.AddSerializationRule(memberInfo,rule);
            }

            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(IFormatProvider culture)
        {
            if (culture is null)
            {
                throw new ArgumentNullException(nameof(culture));
            }
            Using(rule => ((IFormattable)rule).ToString(null, culture));
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TType>.ParentConfig => printingConfig;
    }
}