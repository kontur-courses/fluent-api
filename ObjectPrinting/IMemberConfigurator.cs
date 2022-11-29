using System;
using System.Globalization;

namespace ObjectPrinting;

public interface IMemberConfigurator<TOwner, T>
{
    IBasicConfigurator<TOwner> BasicConfigurator { get; }
    // IBasicConfigurator<TOwner> Using<TOwner>(CultureInfo cultureInfo);
    IBasicConfigurator<TOwner> Configure(CultureInfo cultureInfo);
}