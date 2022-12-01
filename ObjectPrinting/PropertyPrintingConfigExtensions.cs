using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> Cut<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            propConfig.Parent.MaxStringLength[propConfig.MemberInfo] = maxLen;
            return propConfig.Parent;
        }
    }
}
