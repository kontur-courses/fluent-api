namespace ObjectPrinting;

public class PropertyConfig<TOwner, TValue>
{
    internal PrintingConfig<TOwner> Owner { get; }
    internal string PropertyName { get; }
    
    internal PropertyConfig(
        PrintingConfig<TOwner> owner,
        string propertyName)
    {
        Owner = owner;
        PropertyName = propertyName;
    }
}