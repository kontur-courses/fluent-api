namespace ObjectPrinting.Configuration
{
    public interface IPropertySerializer<in TProperty>
    {
        string Serialize(TProperty value);
    }
}