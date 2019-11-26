using System;
using System.Linq.Expressions;
using System.Reflection;
using ObjectPrinting.Serializer;

namespace ObjectPrinting.PrintingConfigs
{
    public class PropertyPrintingConfig<TOwner, T> : IPropertyPrintingConfig<TOwner>
    {
        private readonly PrintingConfig<TOwner> config;
        private readonly Action<PropertySerializationRule> applyNewRuleToConfig;
        private readonly SerializationFilter propFilter;

        public PropertyPrintingConfig(PrintingConfig<TOwner> owner)
        {
            config = owner;
            applyNewRuleToConfig = (config as IPrintingConfig).ApplyNewSerializationRule;
            propFilter = (obj, propertyInfo) => propertyInfo.PropertyType == typeof(T);
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> owner, Expression<Func<TOwner, T>> filter)
        : this(owner)
        {
            var propInfo = ((MemberExpression)filter.Body).Member as PropertyInfo;
            var typeFilter = propFilter;

            propFilter = (obj, propertyInfo) => propertyInfo.Name == propInfo?.Name && typeFilter(obj, propInfo);
        }

        public PrintingConfig<TOwner> Using(Func<T, string> func)
        {
            applyNewRuleToConfig(
                new PropertySerializationRule(propFilter,
                (obj, propertyInfo) => func((T) propertyInfo.GetValue(obj))));
            return config;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.Config => config;
        SerializationFilter IPropertyPrintingConfig<TOwner>.Filter => propFilter;
    }
}