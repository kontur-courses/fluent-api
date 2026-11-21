using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting.Solved
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly HashSet<MemberInfo> targetMembers;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, HashSet<MemberInfo> targetMembers)
        {
            this.printingConfig = printingConfig;
            this.targetMembers = targetMembers;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            foreach (var member in targetMembers)
            {
                printingConfig.CustomSerializers[member] = value => print((TPropType)value);
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