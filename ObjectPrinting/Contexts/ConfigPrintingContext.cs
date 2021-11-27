using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting.Contexts
{
    public class ConfigPrintingContext<TOwner>
    {
        private readonly PrintingConfig config;

        public ConfigPrintingContext(PrintingConfig config)
        {
            this.config = config;
        }

        public ConfigPrintingContext<TOwner> Excluding<TPropType>() =>
            new(config with
            {
                ExcludingTypes = config.ExcludingTypes.Add(typeof(TPropType))
            });

        public ConfigPrintingContext<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> propertySelector)
        {
            var memberInfo = ValidateSelectedMember(propertySelector);
            return new ConfigPrintingContext<TOwner>(config with
            {
                ExcludingMembers = config.ExcludingMembers.Add(memberInfo)
            });
        }

        public TypePrintingContext<TOwner, TPropType> Printing<TPropType>() => new(config);

        public PropertyPrintingContext<TOwner> Printing<TPropType>(Expression<Func<TOwner, TPropType>> propertySelector)
        {
            var memberInfo = ValidateSelectedMember(propertySelector);
            return new PropertyPrintingContext<TOwner>(config, memberInfo);
        }

        public ConfigPrintingContext<TOwner> FormatFor<TPropType>(string format, IFormatProvider formatProvider)
            where TPropType : IFormattable =>
            new(config with
            {
                TypePrinting =
                config.TypePrinting.SetItem(typeof(TPropType), obj => ((TPropType)obj).ToString(format, formatProvider))
            });

        public ConfigPrintingContext<TOwner> MaxStringLength(int maxLength)
        {
            var ellipsis = '\u2026'.ToString();
            Func<object, string> trim = obj =>
            {
                var line = (string)obj;
                return line.Length <= maxLength
                    ? line
                    : line[..maxLength] + ellipsis;
            };

            return new ConfigPrintingContext<TOwner>(config with
            {
                TypePrinting = config.TypePrinting.SetItem(typeof(string), trim)
            });
        }

        public string PrintToString(TOwner obj) => new ObjectPrinter<TOwner>(config).PrintToString(obj);

        private MemberInfo ValidateSelectedMember<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberInfo = (memberSelector.Body as MemberExpression)?.Member;
            if (memberInfo == null || !config.PrintingMemberFactory.CanConvert(memberInfo))
                throw new ArgumentException(
                    $"Selected member must be one of: {string.Join(", ", config.PrintingMemberFactory.SupportedTypes)}.");

            return memberInfo;
        }
    }
}