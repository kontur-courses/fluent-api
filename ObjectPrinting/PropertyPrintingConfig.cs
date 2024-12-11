using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Reflection;

namespace ObjectPrinting.Solved
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
                obj is IFormattable formattable && culture != null
                    ? formattable.ToString(null, culture)
                    : obj.ToString());
        }

        private PrintingConfig<TOwner> AddAlternativeSerializationMethod(Func<object, string> serializator)
        {
            if (member is not null)
            {
                return new PrintingConfig<TOwner>(printingConfig.Settings with
                {
                    MethodsForSerializingPropertiesAndFields = printingConfig.Settings.MethodsForSerializingPropertiesAndFields.Add(
                        member,
                        serializator)
                });
            }
            if (type != null)
            {
                return new PrintingConfig<TOwner>(printingConfig.Settings with
                {
                    AlternativeSerialization = printingConfig.Settings.AlternativeSerialization.Add(
                        typeof(TPropType),
                        serializator)
                });
            }
            
            throw new InvalidOperationException();
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}