using System.Globalization;

namespace ObjectPrinting;

public class MemberConfigurator<TOwner, T> : IMemberConfigurator<TOwner, T>
{
    public IBasicConfigurator<TOwner> BasicConfigurator { get; }
    public IBasicConfigurator<TOwner> Configure(CultureInfo cultureInfo)
    {
        return new ObjectConfigurator<TOwner>();
    }

    public MemberConfigurator(IBasicConfigurator<TOwner> basicConfigurator)
    {
        BasicConfigurator = basicConfigurator;
    }
}