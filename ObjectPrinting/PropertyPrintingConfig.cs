using System;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly string propertyName;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, Expression<Func<TOwner, TPropType>> member)
        {
            propertyName = ((MemberExpression) member.Body).Member.Name;
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (propertyName is null)
                ((IPrintingConfig<TOwner>) printingConfig).TypePrintingFunctions[typeof(TPropType)] = print;
            else
            {
                ((IPrintingConfig<TOwner>) printingConfig).PropNamesPrintingFunctions[propertyName] = print;
            }

            return printingConfig;
        }


        public void SetCulture(Type type, CultureInfo cultureInfo)
        {
            ((IPrintingConfig<TOwner>) printingConfig).NumberTypesToCulture[type] = cultureInfo;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
        string IPropertyPrintingConfig<TOwner, TPropType>.PropertyName => propertyName;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        string PropertyName { get; }
    }
}