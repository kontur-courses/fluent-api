using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertySerializationConfig<TOwner, TTarget> : IPropertyPrintingConfig<TOwner>
    {
        protected PrintingConfig<TOwner> config;
        protected PropertyInfo propertyInfo;
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.Config => config;

        public PropertySerializationConfig(PrintingConfig<TOwner> config,
            Expression<Func<TOwner, TTarget>> propertyProvider)
        {
            this.config = config;
            this.propertyInfo = ((MemberExpression) propertyProvider.Body).Member as PropertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TTarget, string> serializer)
        {
            config.PropertySerializators.Add(propertyInfo, PropertySerializer.CreateSerializer(propertyInfo, serializer));
            return config;
        }
    }
}