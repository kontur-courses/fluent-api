using System;

namespace ObjectPrinting
{
    public class PrinterConfigSerialization<TOwner, TSerializator> : IPrintingConfig
    {
        private readonly PrintingConfig<TOwner> _parent;
        private Func<TSerializator, string> _serializer;

        public PrinterConfigSerialization(PrintingConfig<TOwner> parent)
        {
            _parent = parent;
        }


        public string PrintObject(object obj)
        {
            return _serializer((TSerializator)obj);
        }

        public PrinterConfigSerialization<TOwner, TSerializator> SetSerialization(
            Func<TSerializator, string> serializer)
        {
            _serializer = serializer;
            return this;
        }

        public PrintingConfig<TOwner> ApplyConfig()
        {
            return _parent;
        }
    }
}