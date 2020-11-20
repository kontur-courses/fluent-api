using System.Reflection;

namespace ObjectPrinting.Configuration
{
    public class SelectedProperty<TOwner, TProperty> : IConfigurable<TOwner, TProperty>
    {
        public SelectedProperty(PropertyInfo propertyInfo, PrintingConfig<TOwner> parent)
        {
            PropertyInfo = propertyInfo;
            Owner = parent;
        }

        public PropertyInfo PropertyInfo { get; }
        public PrintingConfig<TOwner> Owner { get; }

        public PrintingConfig<TOwner> Using(IPropertySerializer<TProperty> serializer)
        {
            return Owner;
        }
    }
}