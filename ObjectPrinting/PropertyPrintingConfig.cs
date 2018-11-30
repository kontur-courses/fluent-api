using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly MemberInfo memberInfo;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, Expression<Func<TOwner, TPropType>> member)
        {
            memberInfo = ((MemberExpression) member.Body).Member;
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            if (memberInfo is null)
                ((IPrintingConfig<TOwner>) printingConfig).PrintingConfigData.TypePrintingFunctions[typeof(TPropType)] =
                    print;
            else
            {
                ((IPrintingConfig<TOwner>) printingConfig).PrintingConfigData.PropNamesPrintingFunctions[memberInfo] =
                    print;
            }

            return printingConfig;
        }


        public void SetCulture(Type type, CultureInfo cultureInfo)
        {
            ((IPrintingConfig<TOwner>) printingConfig).PrintingConfigData.NumberTypesToCulture[type] = cultureInfo;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
        MemberInfo IPropertyPrintingConfig<TOwner, TPropType>.MemberInfo => memberInfo;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        MemberInfo MemberInfo { get; }
    }
}