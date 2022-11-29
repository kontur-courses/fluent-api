namespace ObjectPrinting;

public interface IMemberConfigurator<TOwner, T>
{
    IBasicConfigurator<TOwner> BasicConfigurator { get; }
}