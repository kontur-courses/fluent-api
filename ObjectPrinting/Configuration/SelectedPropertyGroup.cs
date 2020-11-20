namespace ObjectPrinting.Configuration
{
    public class SelectedPropertyGroup<TOwner, TProperty> : IConfigurable<TOwner, TProperty>
    {
        public SelectedPropertyGroup(PrintingConfig<TOwner> parent)
        {
            Owner = parent;
        }

        public PrintingConfig<TOwner> Owner { get; }

        public PrintingConfig<TOwner> Using(IPropertySerializer<TProperty> serializer)
        {
            return Owner;
        }
    }
}