using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertySerializingConfig<TOwner, TPropType> : IPropertySerializingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> parentConfig;
        private readonly MemberInfo propertyInfo;
        PrintingConfig<TOwner> IPropertySerializingConfig<TOwner>.ParentConfig => parentConfig;

        public PropertySerializingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.parentConfig = printingConfig;
        }

        public PropertySerializingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo propertyInfo)
        {
            this.parentConfig = printingConfig;
            this.propertyInfo = propertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> serializationMethod)
        {
            if (propertyInfo == null)
            {
                (parentConfig as IPrintingConfig).typeSerialisation[typeof(TPropType)] = BuildFuncSerial(serializationMethod);
                return parentConfig;
            }

            (parentConfig as IPrintingConfig).propertySerialisation[propertyInfo] = BuildFuncSerial(serializationMethod);
            return parentConfig;
        }

        private static Func<object, string> BuildFuncSerial(Func<TPropType, string> funcSerial)
        {
            return obj =>
            {
                if (obj == null)
                    return funcSerial((TPropType)obj);
                if (obj is TPropType propType)
                    return funcSerial(propType);
                throw new ArgumentException($"Cannot be casted to {nameof(TPropType)}");
            };
        }
    }
}