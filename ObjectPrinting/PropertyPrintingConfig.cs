using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly Type? type;
        private readonly MemberInfo? member;
        private readonly PrintingConfig<TOwner> printingConfig;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, MemberInfo member)
        {
            this.printingConfig = printingConfig;
            this.member = member;
        }
        
        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, Type type)
        {
            this.type = type;
            this.printingConfig = printingConfig;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
        {
            return AddAlternativeSerializationMethod(obj => print((TPropType)obj));
        }

        public PrintingConfig<TOwner> Using(CultureInfo? culture)
        {
            return AddAlternativeSerializationMethod(obj =>
                obj is IFormattable formatted && culture != null
                    ? formatted.ToString(null, culture)
                    : obj.ToString());
        }

        private PrintingConfig<TOwner> AddAlternativeSerializationMethod(Func<object, string?> serializer)
        {
            if (member is not null)
            {
                return new PrintingConfig<TOwner>(printingConfig.Settings with
                {
                    AlternativeSerializationOfFieldsAndProperties = printingConfig.Settings.AlternativeSerializationOfFieldsAndProperties.Add(
                        member,
                        serializer)
                });
            }
            if (type is not null)
            {
                return new PrintingConfig<TOwner>(printingConfig.Settings with
                {
                    AlternativeTypeSerialization = printingConfig.Settings.AlternativeTypeSerialization.Add(
                        typeof(TPropType),
                        serializer)
                });
            }
            
            throw new InvalidOperationException();
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }
}