using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting.PrintingConfigs
{
    public static class PropertyPrintingConfigExtensions
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(
            this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            var propInfo = propConfig.GetPropInfo();
            var parentConfig = ((IPropertyPrintingConfig<TOwner, string>) propConfig).ParentConfig;
            parentConfig.trimLengthByName[propInfo.Name] = maxLen;
            return parentConfig;
        }

    }
}
