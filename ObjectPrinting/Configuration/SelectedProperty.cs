using System.Collections.Generic;

namespace ObjectPrinting.Configuration
{
    public class SelectedProperty<TOwner, TProperty> : IPropertyConfigurator<TOwner, TProperty>
    {
        public SelectedProperty(SerializationTarget target, PrintingConfig<TOwner> parent)
        {
            Target = target;
            Owner = parent;
        }

        public IPropertySerializer<TProperty> AppliedSerializer { get; private set; }
        public SerializationTarget Target { get; }
        public PrintingConfig<TOwner> Owner { get; }

        public PrintingConfig<TOwner> Using(IPropertySerializer<TProperty> serializer)
        {
            AppliedSerializer = serializer;
            return Owner;
        }

        IReadOnlyList<SerializationTarget> IPropertyConfigurator.Targets => new[] {Target};
    }
}