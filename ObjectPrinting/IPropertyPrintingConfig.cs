using System;
using System.Globalization;

namespace ObjectPrinting
{
    public interface IPropertyPrintingConfig<TOwner, out TPropType>
    {
        PrintingConfig<TOwner> Using(Func<TPropType, string> print);
        PrintingConfig<TOwner> Using(CultureInfo culture);
    }
}