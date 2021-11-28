#nullable enable
using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly MemberInfo memberName;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PropertyPrintingConfig(MemberInfo memberName, PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
            this.memberName = memberName;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (memberName != null)
            {
                PrintingConfig.MemberConverters[memberName] = obj => print.Invoke((TPropType)obj);
            }
            else
            {
                PrintingConfig.TypeConverters[typeof(TPropType)] = obj => print.Invoke((TPropType)obj);
            }

            return PrintingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => PrintingConfig;

        public PrintingConfig<TOwner> PrintingConfig => printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}