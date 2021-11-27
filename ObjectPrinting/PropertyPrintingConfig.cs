#nullable enable
using System;
using System.Globalization;
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
                printingConfig.MemberConverters[memberName] = obj => print.Invoke((TPropType)obj);
            else
                printingConfig.TypeConverters[typeof(TPropType)] = obj => print.Invoke((TPropType)obj);

            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}