using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyConfig<TOwner, TProperty>
    {
        private readonly PrintingConfig<TOwner> config;
        private readonly PropertyInfo configuratedProperty;

        public PropertyConfig(PrintingConfig<TOwner> config, PropertyInfo configuratedProperty)
        {
            this.config = config;
            this.configuratedProperty = configuratedProperty;
        }

        public PrintingConfig<TOwner> SerializeAs(Func<TProperty, string> f)
        {
            config.AddPropertySerialization(f, configuratedProperty);
            return config;
        }
    }
}