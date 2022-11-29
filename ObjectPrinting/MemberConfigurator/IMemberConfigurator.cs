using System.Globalization;
using ObjectPrinting.BasicConfigurator;

namespace ObjectPrinting.MemberConfigurator;

public interface IMemberConfigurator<TOwner, T>
{
    IBasicConfigurator<TOwner> Configure(CultureInfo cultureInfo);
    IBasicConfigurator<TOwner> Configure(int length);
}