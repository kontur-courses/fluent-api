using System;

namespace ObjectPrinting
{
    public class FinalTypesPrinterHelper : IPrinterHelper<object>
    {
        public string Print<TOwner>(PrintingConfig<TOwner> printer, object obj, int nestingLevel = 0)
        {
            var valueToPrint = obj is IFormattable objFormattable
                ? objFormattable.ToString(null, printer.CurrentCulture)
                : obj.ToString();
            return valueToPrint + Environment.NewLine;
        }
    }
}