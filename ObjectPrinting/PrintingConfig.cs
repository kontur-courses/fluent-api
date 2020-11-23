using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private ImmutableDictionary<MemberInfo, MethodInfo> alternateMemberSerializers;
        private ImmutableDictionary<Type, MethodInfo> alternateTypeSerializers;
        private ImmutableDictionary<Type, CultureInfo> cultureInfos;
        private ImmutableHashSet<MemberInfo> excludingMembers;
        private ImmutableHashSet<Type> excludingTypes;
        private ImmutableDictionary<MemberInfo, int> memberLengths;

        public PrintingConfig()
        {
            excludingTypes = ImmutableHashSet<Type>.Empty;
            excludingMembers = ImmutableHashSet<MemberInfo>.Empty;
            memberLengths = ImmutableDictionary<MemberInfo, int>.Empty;
            alternateTypeSerializers = ImmutableDictionary<Type, MethodInfo>.Empty;
            alternateMemberSerializers = ImmutableDictionary<MemberInfo, MethodInfo>.Empty;
            cultureInfos = ImmutableDictionary<Type, CultureInfo>.Empty;
        }

        private PrintingConfig(PrintingConfig<TOwner> oldPrintingConfig)
        {
            excludingTypes = oldPrintingConfig.excludingTypes;
            excludingMembers = oldPrintingConfig.excludingMembers;
            memberLengths = oldPrintingConfig.memberLengths;
            alternateTypeSerializers = oldPrintingConfig.alternateTypeSerializers;
            alternateMemberSerializers = oldPrintingConfig.alternateMemberSerializers;
            cultureInfos = oldPrintingConfig.cultureInfos;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0, new HashSet<object>());
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

        public PrintingConfig<TOwner> SetAlternateSerialize<TPropType>(Func<TPropType, string> alternateSerialize)
        {
            return new PrintingConfig<TOwner>(this)
            {
                alternateTypeSerializers = alternateTypeSerializers.Add(typeof(TPropType), alternateSerialize.Method)
            };
        }

        public PrintingConfig<TOwner> SetTrimming(
            Expression<Func<TOwner, string>> memberSelector,
            int maxLength)
        {
            var selectedMember = ((MemberExpression) memberSelector.Body).Member;
            return new PrintingConfig<TOwner>(this)
            {
                memberLengths = memberLengths.Add(selectedMember, maxLength)
            };
        }

        public PrintingConfig<TOwner> SetAlternateSerialize<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector,
            Func<TPropType, string> serializer)
        {
            var selectedMember = ((MemberExpression) memberSelector.Body).Member;
            return new PrintingConfig<TOwner>(this)
            {
                alternateMemberSerializers = alternateMemberSerializers.Add(selectedMember, serializer.Method)
            };
        }

        public PrintingConfig<TOwner> SetCultureInfo<TPropType>(CultureInfo cultureInfo)
            where TPropType : IFormattable
        {
            return new PrintingConfig<TOwner>(this)
            {
                cultureInfos = cultureInfos.Add(typeof(TPropType), cultureInfo)
            };
        }

        private string PrintToString(object obj, int nestingLevel, HashSet<object> serializedObjects)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;
            if (!serializedObjects.Add(obj))
                throw new SerializationException("Circular reference");
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(obj.GetType().Name);
            foreach (var memberInfo in obj.GetType()
                .GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.IsPropertyOrField())
                .Where(IsNotExcluded))
            {
                var memberValue = memberInfo.GetValue(obj);
                var memberType = memberInfo.GetValueType();
                var toPrint = TrySerializeMember(memberInfo, memberValue, memberType, out var serializedMember)
                    ? serializedMember
                    : memberValue;
                if (memberLengths.TryGetValue(memberInfo, out var maxLength))
                    toPrint = toPrint.ToString()?.Substring(0, maxLength);
                sb.Append(identation + memberInfo.Name + " = "
                          + PrintToString(toPrint, nestingLevel + 1, serializedObjects));
            }

            return sb.ToString();
        }

        private bool TrySerializeMember(MemberInfo memberInfo, object value, Type valueType,
            out string serializedMember)
        {
            if (alternateMemberSerializers.TryGetValue(memberInfo, out var method))
            {
                serializedMember = method.Invoke(Activator.CreateInstance(method.DeclaringType!),
                    new[] {value})?.ToString();
                return true;
            }

            if (alternateTypeSerializers.TryGetValue(valueType, out method))
            {
                serializedMember = method.Invoke(Activator.CreateInstance(method.DeclaringType!),
                    new[] {value})?.ToString();
                return true;
            }

            if (cultureInfos.TryGetValue(valueType, out var cultureInfo))
            {
                serializedMember = ((IFormattable) value).ToString(null, cultureInfo);
                return true;
            }

            serializedMember = null;
            return false;
        }

        private bool IsNotExcluded(MemberInfo memberInfo)
        {
            return !excludingMembers.Contains(memberInfo) && !excludingTypes.Contains(memberInfo.GetValueType());
        }
    }
}