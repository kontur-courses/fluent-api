using System;

namespace ObjectPrinting
{
    public class PrinterConfigSerialization<TOwner, TSer> : IPrintingConfig
    {
        private Func<TSer, string> _serializer;
        private readonly PrintingConfig<TOwner> _parent;

        public PrinterConfigSerialization(PrintingConfig<TOwner> parent)
        {
            _parent = parent;
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

        public string PrintObject(object obj)
        {
            return _serializer((TSer)obj);
        }
    }
}