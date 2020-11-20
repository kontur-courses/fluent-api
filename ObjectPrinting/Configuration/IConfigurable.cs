namespace ObjectPrinting.Configuration
{
    public interface IConfigurable<TOwner, out TProperty>
    {
        PrintingConfig<TOwner> Using(IPropertySerializer<TProperty> serializer);
    }
}