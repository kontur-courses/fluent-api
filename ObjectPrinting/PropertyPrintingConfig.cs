using System;
using System.Collections;
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
        private Dictionary<Type, object> serializedTypes;
        private Dictionary<MemberInfo, object> serializedProperties;
        
        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo memberInfo, 
            Dictionary<Type, object> serializedTypes,  Dictionary<MemberInfo, object> serializedProperties)
        {
            this.printingConfig = printingConfig;
            this.memberInfo = memberInfo;
            this.serializedTypes = serializedTypes;
            this.serializedProperties = serializedProperties;
        }

        public PrintingConfig<TOwner> Using(Func<TProperty, string> func)
        {
            if (memberInfo != null)
            {
                UsingToProperty(func);
                return new PrintingConfig<TOwner>(printingConfig, serializedProperties);
            }
            UsingToType(func);
            return new PrintingConfig<TOwner>(printingConfig, serializedTypes);
        }
        
        public PrintingConfig<TOwner> Using(CultureInfo cultureInfo)
        {
            if (!serializedTypes.ContainsKey(typeof(TProperty)))
                serializedTypes.Add(typeof(TProperty), cultureInfo);
            else
                serializedTypes[typeof(TProperty)] = cultureInfo;
            
            return new PrintingConfig<TOwner>(printingConfig, serializedTypes);
        }

        private void UsingToProperty(Func<TProperty, string> func)
        {
            if (!serializedProperties.ContainsKey(memberInfo))
                serializedProperties.Add(memberInfo, func);
            else
                serializedProperties[memberInfo] = Delegate.Combine((Delegate)serializedProperties[memberInfo], func);
        }
        
        private void UsingToType(Func<TProperty, string> func)
        {
            if (!serializedTypes.ContainsKey(typeof(TProperty)))
                serializedTypes.Add(typeof(TProperty), func);
            else
            {
                serializedTypes[typeof(TProperty)] = func;
            }
        }
    }
}