using System;
using ObjectPrinting.Contracts;

namespace ObjectPrinting.Contexts
{
    public class TypePrintingConfig<TOwner, TType> : IContextPrintingConfig<TOwner, TType>
    {
        private PrintingConfig<TOwner> PrintingConfig { get; }
        public TypePrintingConfig(PrintingConfig<TOwner> type) => PrintingConfig = type;

        public PrintingConfig<TOwner> Using(Func<TType, string> print)
        {
            var newConfig = (PrintingConfig as IPrintingConfig)
                .AddAlternativePrintingFor(typeof(TType), obj => print((TType) obj));
            return newConfig as PrintingConfig<TOwner>;
        }
    }
}