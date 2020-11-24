using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting.Infrastructure
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly MemberInfo selectedMember;
        private readonly Type selectedType;

        private PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo selectedMember) : this(printingConfig)
        {
            this.selectedMember = selectedMember;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, Type selectedType) : this(printingConfig)
        {
            this.selectedType = selectedType;
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
            printingConfig.GetSettings(typeof(TPropType)).Printer = print;
        }
        
        private void AddMemberPrinter(Func<TPropType, string> print)
        {
            printingConfig.GetSettings(selectedMember).Printer = print;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;

        public void SetLengthConstraint(int length)
        {
            if (selectedMember is null)
                AddTypeLengthConstraint(length);
            else
                AddMemberLengthConstraint(length);
        }
        
        private void AddTypeLengthConstraint(int length)
        {
            printingConfig.GetSettings(typeof(TPropType)).MaxLength = length;
        }
        
        private void AddMemberLengthConstraint(int length)
        {
            printingConfig.GetSettings(selectedMember).MaxLength = length;
        }
    }
}