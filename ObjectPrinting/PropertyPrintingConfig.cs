using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;
        private readonly Expression<Func<TOwner, TPropType>> selector;
        private readonly MemberInfo currentMemberInfo;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig,
            Expression<Func<TOwner, TPropType>> selector)
        {
            var memberExp = selector.Body as MemberExpression;

            this.printingConfig = printingConfig;
            this.selector = selector;
            currentMemberInfo = memberExp.Member;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
        {
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            var serializationConfig = printingConfig as ISerializationConfig<TOwner>;
            if (selector != null)
            {
                var compiledSelector = selector.Compile();
                serializationConfig.SetPropertySerialization(
                    currentMemberInfo, element => print(compiledSelector(element)));
            }
            else
            {
                serializationConfig.SetTypeSerialization<TPropType>(obj => print(obj));
            }

            return printingConfig;
        }

        public PrintingConfig<TOwner> Using(CultureInfo culture)
        {
            var serializationConfig = printingConfig as ISerializationConfig<TOwner>;
            serializationConfig.SetCulture<TPropType>(culture);
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
        MemberInfo IPropertyPrintingConfig<TOwner, TPropType>.MemberInfo => currentMemberInfo;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        MemberInfo MemberInfo { get; }
    }
}