using System;
using System.Linq.Expressions;
using System.Threading;

namespace ObjectPrinting
{
    public partial class PrintingConfig<TOwner>
    {
        public PropertyPrintingConfig<TOwner, TProp> Print<TProp>
            (Expression<Func<TOwner, TProp>> selector)
        {
            var member = ReflectionHelpers.GetMemberFromExpression(selector);

            Predicate predicate = info => info.Name == member.Name;

            var propertyConfig =
                new PropertyPrintingConfig<TOwner, TProp>(this, configuration, predicate);

            return propertyConfig;
        }  

        public PropertyPrintingConfig<TOwner, TType> Print<TType>()
        {
            Predicate predicate = info => info.PropertyType == typeof(TType); 

            var propertyConfig = 
                new PropertyPrintingConfig<TOwner, TType>(this, configuration, predicate);

            return propertyConfig;
        }
    }
}
