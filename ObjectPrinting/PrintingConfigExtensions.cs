using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ObjectPrinting
{
    public static class PrintingConfigExtensions
    {
        public static IPrintingPropertyConfig<TOwner, string> SetMaxLength<TOwner>(this IPrintingPropertyBaseConfig<TOwner, string> propConfig, int length)
        {
            var root = propConfig.Root;
            var stringPropertyLengths = root.MaxStringPropertyLengths;
            if (!stringPropertyLengths.ContainsKey(propConfig.CurrentProperty))
                stringPropertyLengths.Add(propConfig.CurrentProperty, length);
            else stringPropertyLengths[propConfig.CurrentProperty] = length;

            return new PrintingPropertyConfig<TOwner, string>(propConfig.CurrentProperty, propConfig.Root);
        }
    }
}