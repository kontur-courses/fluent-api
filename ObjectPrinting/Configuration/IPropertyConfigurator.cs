using System.Collections.Generic;
using ObjectPrinting.Serializers;

namespace ObjectPrinting.Configuration
{
    public interface IPropertyConfigurator
    {
        IReadOnlyList<SerializationTarget> Targets { get; }
    }

    public interface IPropertyConfigurator<TOwner, TProperty> : IPropertyConfigurator
    {
        PropertySerializer<TProperty> AppliedSerializer { get; }
        PrintingConfigBuilder<TOwner> Using(PropertySerializer<TProperty> serializer);
    }
}