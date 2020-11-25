using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        internal readonly MemberInfo memberInfo;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo memberInfo = null)
        {
            this.printingConfig = printingConfig;
            this.memberInfo = memberInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (memberInfo == null)
                printingConfig.typeMemberConfigs[typeof(TPropType)] = print;
            else
                printingConfig.nameMemberConfigs[memberInfo.Name] = print;
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}