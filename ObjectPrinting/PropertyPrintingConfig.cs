using System;
using System.Reflection;

namespace ObjectPrinting
{
    interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        Type SelectedType { get; }
        PropertyInfo SelectedProperty { get; }
        PrintingConfig<TOwner> Parent { get; }
    }
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.Parent => parent;
        private readonly PrintingConfig<TOwner> parent;

        PropertyInfo IPropertyPrintingConfig<TOwner, TPropType>.SelectedProperty => selectedProperty;
        private readonly PropertyInfo selectedProperty;

        Type IPropertyPrintingConfig<TOwner, TPropType>.SelectedType => selectedType;
        private readonly Type selectedType;

		public PropertyPrintingConfig(PrintingConfig<TOwner> parent, Type selectedType)
        {
            this.parent = parent;
            this.selectedType = selectedType;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> parent, PropertyInfo selectedProperty)
	    {
	        this.parent = parent;
	        this.selectedProperty = selectedProperty;
	    }


	    public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
	    {
	        var parentConfig = parent as IPrintingConfig;
            if (selectedType != null)
                parentConfig.CustomSerialization.Add(typeof(TPropType), obj => print((TPropType) obj));
            else
                parentConfig.CustomPropetrySerialization.Add(selectedProperty, obj => print((TPropType)obj));
            return parent;
		}
	}
}