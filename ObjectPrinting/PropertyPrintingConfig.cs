using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using ObjectPrinting.Interfaces;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly List<MemberInfo> selectedMemberInfos = new();

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, IEnumerable<MemberInfo> memberInfos)
        {
            selectedMemberInfos.AddRange(memberInfos);
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            return selectedMemberInfos.Count > 0
                ? printingConfig.Using(selectedMemberInfos, print)
                : printingConfig.Using(print);
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            return printingConfig.Using<TPropType>(culture);
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }
}