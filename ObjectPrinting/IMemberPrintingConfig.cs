using System;

namespace ObjectPrinting
{
    public interface IMemberPrintingConfig<TOwner, T>
    {
        public PrintingConfig<TOwner> ParentConfig { get; }
        public PrintingConfig<TOwner> Using(Func<T, string> serializationRule);
    }
}
