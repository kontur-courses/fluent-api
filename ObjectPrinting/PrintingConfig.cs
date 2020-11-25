using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        private ImmutableDictionary<MemberInfo, Func<object, string>> alternateMemberSerializers =
            ImmutableDictionary<MemberInfo, Func<object, string>>.Empty;

        private ImmutableDictionary<Type, Func<object, string>> alternateTypeSerializers =
            ImmutableDictionary<Type, Func<object, string>>.Empty;

        private ImmutableHashSet<MemberInfo> excludingMembers = ImmutableHashSet<MemberInfo>.Empty;
        private ImmutableHashSet<Type> excludingTypes = ImmutableHashSet<Type>.Empty;
        private ImmutableDictionary<MemberInfo, int> memberLengths = ImmutableDictionary<MemberInfo, int>.Empty;

        public PrintingConfig()
        {
        }

        private PrintingConfig(PrintingConfig<TOwner> oldPrintingConfig)
        {
            excludingTypes = oldPrintingConfig.excludingTypes;
            excludingMembers = oldPrintingConfig.excludingMembers;
            memberLengths = oldPrintingConfig.memberLengths;
            alternateTypeSerializers = oldPrintingConfig.alternateTypeSerializers;
            alternateMemberSerializers = oldPrintingConfig.alternateMemberSerializers;
        }

        public string PrintToString(TOwner obj)
        {
            return Serializer<TOwner>.Serialize(this, obj);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            return new PrintingConfig<TOwner>(this)
            {
                excludingTypes = excludingTypes.Add(typeof(TPropType))
            };
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var selectedMember = ((MemberExpression) memberSelector.Body).Member;
            return new PrintingConfig<TOwner>(this)
            {
                excludingMembers = excludingMembers.Add(selectedMember)
            };
        }

        public PrintingConfig<TOwner> SetAlternateSerialize<TPropType>(Func<TPropType, string> serializer)
        {
            return new PrintingConfig<TOwner>(this)
            {
                alternateTypeSerializers =
                    alternateTypeSerializers.AddOrSet(typeof(TPropType), x => serializer((TPropType) x))
            };
        }

        public PrintingConfig<TOwner> SetTrimming(
            Expression<Func<TOwner, string>> memberSelector,
            int maxLength)
        {
            var selectedMember = ((MemberExpression) memberSelector.Body).Member;
            return new PrintingConfig<TOwner>(this)
            {
                memberLengths = memberLengths.AddOrSet(selectedMember, maxLength)
            };
        }

        public PrintingConfig<TOwner> SetAlternateSerialize<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector,
            Func<TPropType, string> serializer)
        {
            var selectedMember = ((MemberExpression) memberSelector.Body).Member;
            return new PrintingConfig<TOwner>(this)
            {
                alternateMemberSerializers =
                    alternateMemberSerializers.AddOrSet(selectedMember, x => serializer((TPropType) x))
            };
        }

        public PrintingConfig<TOwner> SetCulture<TPropType>(CultureInfo cultureInfo)
            where TPropType : IFormattable
        {
            return new PrintingConfig<TOwner>(this)
            {
                alternateTypeSerializers =
                    alternateTypeSerializers.AddOrSet(
                        typeof(TPropType),
                        x => ((IFormattable) x).ToString(null, cultureInfo))
            };
        }

        public bool TryGetSerializer(MemberInfo memberInfo, out Func<object, string> serializer)
        {
            return alternateMemberSerializers.TryGetValue(memberInfo, out serializer)
                   || alternateTypeSerializers.TryGetValue(memberInfo.GetValueType(), out serializer);
        }

        public bool TryGetMemberLength(MemberInfo memberInfo, out int length)
        {
            return memberLengths.TryGetValue(memberInfo, out length);
        }

        public bool IsMemberNotExcluded(MemberInfo memberInfo)
        {
            return !excludingMembers.Contains(memberInfo) && !excludingTypes.Contains(memberInfo.GetValueType());
        }
    }
}