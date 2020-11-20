using ObjectPrinting.Configuration;

namespace ObjectPrinting.Serializers
{
    public class IgnorePropertySerializer<TProperty> : IPropertySerializer<TProperty>
    {
        public string Serialize(TProperty value) => string.Empty;
    }
}