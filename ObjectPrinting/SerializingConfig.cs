using System;

namespace ObjectPrinting
{
    public class SerializingConfig<TOwner, TPropType> : ISerializingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> _baseClass;
        
        public SerializingConfig(PrintingConfig<TOwner> baseClass)
        {
            _baseClass = baseClass;
        }
        
        public PrintingConfig<TOwner> Using(Func<TPropType, string> param)
        {
            return _baseClass;
        }

        PrintingConfig<TOwner> ISerializingConfig<TOwner>.SerializingConfig => _baseClass;
    }
}