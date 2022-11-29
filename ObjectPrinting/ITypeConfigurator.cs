using System;
using System.Collections.Generic;
using System.Globalization;

namespace ObjectPrinting;

public interface ITypeConfigurator<TOwner, T>
{
    IBasicConfigurator<TOwner> Configure(CultureInfo cultureInfo);
    IBasicConfigurator<TOwner> Configure(int length);
}