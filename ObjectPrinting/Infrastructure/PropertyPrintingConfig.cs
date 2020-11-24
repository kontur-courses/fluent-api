using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting.Infrastructure
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly MemberInfo selectedMember;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo selectedMember)
        {
            this.printingConfig = printingConfig;
            this.selectedMember = selectedMember;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (selectedMember is null)
                AddTypePrinter(print);
            else
                AddMemberPrinter(print);
            return printingConfig;
        }

        private void AddTypePrinter(Func<TPropType, string> print)
        {
            printingConfig.TypePrinters.Add(typeof(TPropType), print);
        }
        private void AddMemberPrinter(Func<TPropType, string> print)
        {
            printingConfig.MemberPrinters.Add(selectedMember, print);
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }
}