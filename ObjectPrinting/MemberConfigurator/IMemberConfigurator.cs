using System.Globalization;
using ObjectPrinting.ObjectConfiguration;

namespace ObjectPrinting.MemberConfigurator;

public interface IMemberConfigurator<TOwner, T>
{
    IObjectConfiguration<TOwner> Configure(CultureInfo cultureInfo);
    IObjectConfiguration<TOwner> Configure(int length);
}