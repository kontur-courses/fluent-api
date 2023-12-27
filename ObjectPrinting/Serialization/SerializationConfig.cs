using System;

namespace ObjectPrinting.Serialization
{
    public class SerializationConfig<TOwner, TSerialization> : ISerializationConfig<TOwner, TSerialization>
    {
        private Func<TSerialization, string> serialize;

        public SerializationConfig(PrintingConfig<TOwner> printingConfig)
        {
            And = printingConfig;
        }

        public PrintingConfig<TOwner> And { get; }

        public IWrap<TOwner> Using(Func<TSerialization, string> serialize)
        {
            if (serialize == null)
                throw new ArgumentNullException();

            this.serialize = serialize;
            return this;
        }

        public IWrap<TOwner> Wrap(Func<string, string> modify)
        {
            if (modify == null)
                throw new ArgumentNullException();

            var currentFunc = serialize;
            serialize = value => modify(currentFunc(value));
            return this;
        }

        public Func<object, string> SerializationFunc =>
            p => serialize((TSerialization)p);
    }
}