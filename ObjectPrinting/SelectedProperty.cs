using System;
using System.Globalization;
using System.Reflection;
using ObjectPrinting.Solved;

namespace ObjectPrinting
{
    public class SelectedProperty<TOwner, TProperty>
    {
        private readonly Config config;
        private readonly PrintingConfig<TOwner> parent;
        private readonly PropertyInfo selectedProperty;

        public SelectedProperty(PropertyInfo selectedProperty, PrintingConfig<TOwner> parent, Config config)
        {
            this.selectedProperty = selectedProperty;
            this.parent = parent;
            this.config = config;
        }

        public PrintingConfig<TOwner> UseSerializer(Func<TProperty, string> func)
        {
            config.fieldSerializers[selectedProperty] = func;
            return parent;
        }

        public PrintingConfig<TOwner> Exclude()
        {
            config.exludedFields.Add(selectedProperty);
            return parent;
        }

        public PrintingConfig<TOwner> SetCulture(CultureInfo cultureInfo)
        {
            var func = new Func<TProperty, string>(x => string.Format(cultureInfo, x.ToString()));

            config.fieldSerializers[selectedProperty] = func;
            return parent;
        }
    }
}