using System;
using System.Linq.Expressions;
using System.Reflection;
using ObjectPrinting.Serializers;

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

        public PropertyPrintingContext<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> propertySelector)
        {
            var memberInfo = ValidateSelectedMember(propertySelector);
            return new PropertyPrintingContext<TOwner, TPropType>(config, memberInfo);
        }

        public ConfigPrintingContext<TOwner> FormatFor<TPropType>(IFormatProvider formatProvider)
            where TPropType : IConvertible =>
            new(config with
            {
                TypePrinting =
                config.TypePrinting.SetItem(typeof(TPropType), obj => ((TPropType)obj).ToString(formatProvider))
            });

        public ConfigPrintingContext<TOwner> MaxStringLength(int maxLength)
        {
            Func<object, string> trim = obj =>
            {
                var line = (string)obj;
                return line.Length <= maxLength
                    ? line
                    : line[..maxLength] + "…";
            };

            return new ConfigPrintingContext<TOwner>(config with
            {
                TypePrinting = config.TypePrinting.SetItem(typeof(string), trim)
            });
        }

        public string PrintToString(TOwner obj) =>
            ((ISerializer)new ObjectSerializer(config)).Serialize(obj).ToString();

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