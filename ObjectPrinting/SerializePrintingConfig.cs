using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public class SerializePrintingConfig<TOwner, T>
    {
        private readonly PrintingConfig<TOwner> oldPrintingConfig;
        private readonly PropertyInfo propertyInfo;
        private readonly Type propertyType;

        public SerializePrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo propertyInfo)
        {
            oldPrintingConfig = printingConfig;
            this.propertyInfo = propertyInfo;
        }

        public SerializePrintingConfig(PrintingConfig<TOwner> printingConfig, Type propertyType)
        {
            oldPrintingConfig = printingConfig;
            this.propertyType = propertyType;
        }

        public PrintingConfig<TOwner> As(Func<T, string> serializer)
        {
            var newConfigs = oldPrintingConfig.Configs;
            object configKey;
            if (propertyInfo == null)
                configKey = propertyType;
            else
                configKey = propertyInfo;
            if (!newConfigs.ContainsKey(configKey))
                newConfigs = newConfigs.Add(configKey, new List<Delegate>());
            newConfigs[configKey].Add(serializer);
            return new PrintingConfig<TOwner>(newConfigs);
        }
    }
}