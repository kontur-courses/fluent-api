using System;

namespace ObjectPrinting.Configs
{
    public interface IPropertyPrintingConfig<TOwner>
    {
        public PrintingConfig<TOwner> PrintingConfig { get; }

        public Func<object, string> Serializer { get; }
    }
}