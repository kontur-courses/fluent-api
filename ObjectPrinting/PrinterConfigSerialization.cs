using System;

namespace ObjectPrinting
{
    public class PrinterConfigSerialization<TOwner, TSer> : IPrintingConfig
    {
        private readonly PrintingConfig<TOwner> _parent;
        private Func<TSer, string> _serializer;

        public PrinterConfigSerialization(PrintingConfig<TOwner> parent)
        {
            _parent = parent;
        }

        public string PrintObject(object obj)
        {
            return _serializer((TSer)obj);
        }

        public PrinterConfigSerialization<TOwner, TSer> SetSerialization(Func<TSer, string> serializer)
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