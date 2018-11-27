using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class SerializePrintingConfig<TOwner, T>
    {
        private string propertyName;
        private Type propertyType;
        private PrintingConfig<TOwner> oldPrintingConfig;

        public SerializePrintingConfig(PrintingConfig<TOwner> printingConfig, string propertyName)
        {
            oldPrintingConfig = printingConfig;
            this.propertyName = propertyName;
        }

        public SerializePrintingConfig(PrintingConfig<TOwner> printingConfig, Type propertyType)
        {
            oldPrintingConfig = printingConfig;
            this.propertyType = propertyType;
        }

        public PrintingConfig<TOwner> As(Func<T, string> serializer)
        {
            Delegate s = serializer;
            var newConfigs = oldPrintingConfig.configs;
            object configKey;
            if (propertyName == null)
                configKey = propertyType;
            else
                configKey = propertyName;
            if (!newConfigs.ContainsKey(configKey))
                newConfigs.Add(configKey, new List<Delegate>());
            newConfigs[configKey].Add(serializer);
            return new PrintingConfig<TOwner>(newConfigs);
        }
    }
}