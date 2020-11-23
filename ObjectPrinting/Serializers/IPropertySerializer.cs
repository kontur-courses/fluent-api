namespace ObjectPrinting.Serializers
{
    /// <summary>
    /// General property serializer
    /// </summary>
    public interface IPropertySerializer<in TProperty>
    {
        string Serialize(TProperty value);
    }
}