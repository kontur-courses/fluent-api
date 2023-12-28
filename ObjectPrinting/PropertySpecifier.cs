using System;
using System.Reflection;
using ObjectPrinting.Interfaces;

namespace ObjectPrinting
{
    public class PropertySpecifier<T, TOwner> : IPropertySpecifier<T, TOwner>
    {
        private PrintingConfig<TOwner> PrintingConfig { get; }
        private readonly PropertyInfo propertyInfo;
        
        public PropertySpecifier(PrintingConfig<TOwner> printingConfig, PropertyInfo propertyInfo)
        {
            PrintingConfig = printingConfig;
            this.propertyInfo = propertyInfo;
        }

        public PropertySpecifier(PrintingConfig<TOwner> printingConfig)
        {
            PrintingConfig = printingConfig;
        }
        
        public PrintingConfig<TOwner> With(Func<T, string> serializer)
        {
            ((ISerializerSetter)PrintingConfig).SetSerializer(serializer, propertyInfo);
            
            return PrintingConfig;
        }
    }
}