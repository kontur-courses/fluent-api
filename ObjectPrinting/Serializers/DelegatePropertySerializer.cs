using System;
using ObjectPrinting.Configuration;

namespace ObjectPrinting.Serializers
{
    public class DelegatePropertySerializer<TProperty> : IPropertySerializer<TProperty>
    {
        private readonly Func<TProperty, string> serializer;

        public DelegatePropertySerializer(Func<TProperty, string> serializer)
        {
            this.serializer = serializer;
        }

        public string Serialize(TProperty value) => serializer.Invoke(value);
    }
}