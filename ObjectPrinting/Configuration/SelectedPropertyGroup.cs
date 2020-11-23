using System.Collections.Generic;
using System.Linq;

namespace ObjectPrinting.Configuration
{
    public class SelectedPropertyGroup<TOwner, TProperty> : IPropertyConfigurator<TOwner, TProperty>
    {
        public SelectedPropertyGroup(IEnumerable<SerializationTarget> targets, PrintingConfig<TOwner> parent)
        {
            Targets = targets.ToArray();
            Owner = parent;
        }

        public IPropertySerializer<TProperty> AppliedSerializer { get; private set; }
        public PrintingConfig<TOwner> Owner { get; }

        public PrintingConfig<TOwner> Using(IPropertySerializer<TProperty> serializer)
        {
            AppliedSerializer = serializer;
            return Owner;
        }

        public IReadOnlyList<SerializationTarget> Targets { get; }
    }
}