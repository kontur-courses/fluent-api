using System;
using System.Reflection;

namespace ObjectPrinting.PrintingConfiguration
{
    public class PropertyPrintingConfig<TOwner, TMemberType>
    {
        public PrintingConfig<TOwner> PrintingConfig { get; }
        public MemberInfo PropertyInfo { get; }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo propertyInfo)
        {
            PrintingConfig = printingConfig;
            PropertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> Exclude()
        {
            PrintingConfig.ExcludeMember(PropertyInfo);
            return PrintingConfig;
        }

        public PrintingConfig<TOwner> ChangeSerialization(Func<TMemberType, string> func)
        {
            PrintingConfig.AddSerializationRule(PropertyInfo, func);
            return PrintingConfig;
        }
    }
}