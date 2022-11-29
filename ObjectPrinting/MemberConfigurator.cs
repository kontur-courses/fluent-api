namespace ObjectPrinting;

public class MemberConfigurator<TOwner, T> : IMemberConfigurator<TOwner, T>
{
    public IBasicConfigurator<TOwner> BasicConfigurator { get; }
        
    public MemberConfigurator(IBasicConfigurator<TOwner> basicConfigurator)
    {
        BasicConfigurator = basicConfigurator;
    }
}