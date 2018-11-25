using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner>
    {
        protected readonly PrintingConfig<TOwner> config;
        protected readonly PropertyInfo propertyInfo;
        protected readonly Type propertyType;
        protected LambdaExpression printingMethod;

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.Config => config;
        PropertyInfo IPropertyPrintingConfig<TOwner>.PropertyInfo => propertyInfo;
        LambdaExpression IPropertyPrintingConfig<TOwner>.PrintingMethod => printingMethod;
        Type IPropertyPrintingConfig<TOwner>.PropertyType => propertyType;

        public PropertyPrintingConfig(PrintingConfig<TOwner> config, PropertyInfo propertyInfo = null)
        {
            this.config = config;
            this.propertyInfo = propertyInfo;
            this.propertyType = typeof(TPropType);
        }

        public PrintingConfig<TOwner> Using(Expression<Func<TPropType, string>> printMethod)
        {
            printingMethod = printMethod;
            return config;
        }
    }

    public interface IPropertyPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> Config { get; }
        PropertyInfo PropertyInfo { get; }
        Type PropertyType { get; }
        LambdaExpression PrintingMethod { get; }
    }
}