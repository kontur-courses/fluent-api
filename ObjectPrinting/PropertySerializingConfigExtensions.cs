using System;
using System.Globalization;

namespace ObjectPrinting
{
    public static class PropertySerializingConfigExtensions 
    {
        public static PrintingConfig<TOwner> CutPrefix<TOwner>(this PropertySerializingConfig<TOwner, string> config,
            int prefixLen)
        {
            var prop = (config as IPropertySerializingConfig<TOwner>).PropertyInfo;
            ((config as IPropertySerializingConfig<TOwner>).ParentConfig
                as IPrintingConfig<TOwner>).MaxLengthOfStringProperty[prop] = prefixLen;
            return (config as IPropertySerializingConfig<TOwner>).ParentConfig;
        }
    }
}