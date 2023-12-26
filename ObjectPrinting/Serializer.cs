using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class Serializer<T, TOwner> : ISerializer<T, TOwner>
    {
        public PrintingConfig<TOwner> PrintingConfig { get; }
        private readonly PropertyInfo propertyInfo;
        
        public Serializer(PrintingConfig<TOwner> printingConfig, PropertyInfo propertyInfo)
        {
            PrintingConfig = printingConfig;
            this.propertyInfo = propertyInfo;
        }

        public Serializer(PrintingConfig<TOwner> printingConfig)
        {
            PrintingConfig = printingConfig;
        }
        
        public PrintingConfig<TOwner> Serialize(Func<T, string> serializer)
        {
            (PrintingConfig as ISerializerSetter).SetSerializer(serializer, propertyInfo);
            
            return PrintingConfig;
        }
    }
}