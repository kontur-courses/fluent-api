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
        IPropertySerializer<TProperty> AppliedSerializer { get; }
        PrintingConfig<TOwner> Using(IPropertySerializer<TProperty> serializer);
    }
}