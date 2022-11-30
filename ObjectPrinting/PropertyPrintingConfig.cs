using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TProperty>
    {
        public PrintingConfig<TOwner> ParentConfig => printingConfig;
        
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly MemberInfo memberInfo;
        private readonly IDictionary<object, object> serializers;
        
        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo memberInfo, IDictionary<object, object> serializers)
        {
            this.printingConfig = printingConfig;
            this.memberInfo = memberInfo;
            this.serializers = serializers;
        }

        public PrintingConfig<TOwner> Using(Func<TProperty, string> func)
        {
            if (memberInfo != null)
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
            var propertyName = memberInfo.Name;
            
            if (!serializers.ContainsKey(propertyName))
                serializers.Add(propertyName, func);
            else
                serializers[propertyName] = Delegate.Combine((Delegate)serializers[propertyName], func);
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