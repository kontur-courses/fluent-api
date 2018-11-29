using System;
using System.Globalization;

namespace ObjectPrinting
{
    public interface IMemberPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> Using(CultureInfo culture);
        PrintingConfig<TOwner> Using(Func<TPropType, string> print);
    }
}