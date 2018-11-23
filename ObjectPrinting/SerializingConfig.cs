using System;

namespace ObjectPrinting
{
    public class SerializingConfig<TOwner, TPropType> : ISerializingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> baseClass;

        public SerializingConfig(PrintingConfig<TOwner> baseClass)
        {
            this.baseClass = baseClass;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> param)
        {
            return baseClass;
        }
        public PrintingConfig<TOwner> Exclude()
        {
            return baseClass;
        }
        PrintingConfig<TOwner> ISerializingConfig<TOwner>.SerializingConfig => baseClass;
    }
}