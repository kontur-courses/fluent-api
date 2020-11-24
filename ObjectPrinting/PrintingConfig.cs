using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly SerializationConfig config = new SerializationConfig();
        private MemberInfo currentMember;

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>() =>
            new PropertyPrintingConfig<TOwner, TPropType>(this);

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            currentMember = ((MemberExpression) memberSelector.Body).Member;
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            currentMember = ((MemberExpression) memberSelector.Body).Member;
            config.ExcludedMembers.Add(currentMember);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            config.ExcludedTypes.Add(typeof(TPropType));
            return this;
        }

        public void SetPrintingRule<TPropType>(Func<TPropType, string> print)
        {
            if (currentMember == null)
                config.TypePrintingRules[typeof(TPropType)] = x => print((TPropType) x);
            else
                config.MemberPrintingRules[currentMember] = x => print((TPropType) x);
            currentMember = null;
        }

        public void SetCulture<TPropType>(CultureInfo culture)
        {
            if (currentMember == null)
                config.TypeCultures[typeof(TPropType)] = culture;
            else
                config.MemberCultures[currentMember] = culture;
            currentMember = null;
        }

        public void SetMaxLength(int maxLen)
        {
            if (currentMember == null)
                config.GlobalTrimToLength = x => x.Substring(0, Math.Min(x.Length, maxLen));
            else
                config.MemberTrimToLength[currentMember] = x => x.Substring(0, Math.Min(x.Length, maxLen));
            currentMember = null;
        }

        public string PrintToString(TOwner obj)
        {
            var serialization = new Serializer(config);
            return serialization.Serialize(obj);
        }
    }
}