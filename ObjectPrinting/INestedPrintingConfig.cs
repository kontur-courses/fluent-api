using System;

namespace ObjectPrinting
{
    public interface INestedPrintingConfig<TOwner, TMember>
    {
        public PrintingConfig<TOwner> Using(Func<TMember, string> customSerializer);
    }
}