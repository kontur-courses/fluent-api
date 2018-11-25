using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class StringPropertyConfig<TOwner, TPropType> : PropertyPrintingConfig<TOwner, TPropType>, IPropertyPrintingConfig<TOwner>
    {
        public int? TrimLength { get; set; }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.Config => config;
        PropertyInfo IPropertyPrintingConfig<TOwner>.PropertyInfo => propertyInfo;
        LambdaExpression IPropertyPrintingConfig<TOwner>.PrintingMethod => printingMethod;

        public StringPropertyConfig(PrintingConfig<TOwner> config, PropertyInfo propertyInfo = null) : base(config, propertyInfo)
        {
        }
    }
}