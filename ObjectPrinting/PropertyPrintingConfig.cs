using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TProperty>
    {
        public PrintingConfig<TOwner> ParentConfig => printingConfig;
        public PropertyInfo PropertyInfo => propertyInfo;
        public IDictionary<object, object> Serializers => serializers;

        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly PropertyInfo propertyInfo;
        private readonly IDictionary<object, object> serializers;
        
        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, PropertyInfo propertyInfo, IDictionary<object, object> serializers)
        {
            this.printingConfig = printingConfig;
            this.propertyInfo = propertyInfo;
            this.serializers = serializers;
        }

        public PrintingConfig<TOwner> Using(Func<TProperty, string> func)
        {
            if (propertyInfo != null)
                UsingToProperty(func);
            else
                UsingToType(func);

            return printingConfig;
        }
        
        public PrintingConfig<TOwner> Using(CultureInfo cultureInfo)
        {
            if (!serializers.ContainsKey(typeof(TProperty)))
                serializers.Add(typeof(TProperty), cultureInfo);
            else
                serializers[typeof(TProperty)] = cultureInfo;
            
            return printingConfig;
        }
        

        private void UsingToProperty(Func<TProperty, string> func)
        {
            var propertyName = propertyInfo.Name;
            
            if (!serializers.ContainsKey(propertyName))
                serializers.Add(propertyName, func);
            else
                serializers[propertyName] = Delegate.Combine(func, (Delegate)serializers[propertyName]);
        }
        
        private void UsingToType(Func<TProperty, string> func)
        {
            if (!serializers.ContainsKey(typeof(TProperty)))
                serializers.Add(typeof(TProperty), func);
            else
            {
                serializers[typeof(TProperty)] = func;
            }
        }
    }
}