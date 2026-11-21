namespace ObjectPrinting;

public static class PropertyConfigExtensions
{
    public static PrintingConfig<TOwner> Trimmed<TOwner>(
        this PropertyConfig<TOwner, string> property,
        int length)
    {
        return property.Owner.Trimmed(property.PropertyName, length);
    }
}