using System.Globalization;
using ObjectPrinting.BasicConfigurator;

namespace ObjectPrinting.TypeConfigurator;

public interface ITypeConfigurator<TOwner, T>
{
    IBasicConfigurator<TOwner> Configure(CultureInfo cultureInfo);
    IBasicConfigurator<TOwner> Configure(int length);
}