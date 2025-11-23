using System;
using System.Globalization;

namespace ObjectPrinting.Interface;

public interface IFormattableMemberConfig<TOwner, TProp>
    where TProp : IFormattable
{
    PrintingConfig<TOwner> Using(CultureInfo culture);
}