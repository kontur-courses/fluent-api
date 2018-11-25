using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class NumberPropertyConfig<TOwner, TPropType> : PropertyPrintingConfig<TOwner, TPropType>, IPropertyPrintingConfig<TOwner>, INumberPrintingConfig
    {
        public CultureInfo CultureInfo { get; set; }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.Config => config;
        PropertyInfo IPropertyPrintingConfig<TOwner>.PropertyInfo => propertyInfo;
        LambdaExpression IPropertyPrintingConfig<TOwner>.PrintingMethod => printingMethod;

        public NumberPropertyConfig(PrintingConfig<TOwner> config, PropertyInfo propertyInfo = null) : base(config, propertyInfo)
        {
        }
    }

    public interface INumberPrintingConfig
    {
        CultureInfo CultureInfo { get; set; }
    }
}