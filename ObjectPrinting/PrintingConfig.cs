using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<MemberInfo> excludedMembers = new HashSet<MemberInfo>();
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();

        private readonly Dictionary<MemberInfo, Func<object, string>> membersSerializesInfos =
            new Dictionary<MemberInfo, Func<object, string>>();

        private readonly Dictionary<Type, Func<object, string>> typeSerializesInfos =
            new Dictionary<Type, Func<object, string>>();

        private int maxRecursion = 2;

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public PrintingConfig<TOwner> WithMaxRecursion(int maxRecursion)
        {
            if (maxRecursion < 1)
                throw new ArgumentException();
            this.maxRecursion = maxRecursion;

            return this;
        }

        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> exclude)
        {
            var memberInfo = GetMemberInfo(exclude);

            excludedMembers.Add(memberInfo);

            return this;
        }

        public PrintingConfig<TOwner> Using<TPropType>(CultureInfo culture)
            where TPropType : IFormattable
        {
            return Using<TPropType>(
                p => p.ToString(null, culture));
        }

        public PrintingConfig<TOwner> Using<TPropType>(Func<TPropType, string> serialize)
        {
            Func<object, string> func = p => serialize((TPropType)p);

            typeSerializesInfos[typeof(TPropType)] = func;

            return this;
        }

        public PrintingConfig<TOwner> Using<TPropType>(
            Expression<Func<TOwner, TPropType>> property,
            Func<TPropType, string> serialize)
        {
            var memberInfo = GetMemberInfo(property);
            Func<object, string> func = p => serialize((TPropType)p);

            membersSerializesInfos[memberInfo] = func;

            return this;
        }

        private static MemberInfo GetMemberInfo<TPropType>(Expression<Func<TOwner, TPropType>> expression)
        {
            var memberExpression = expression.Body is UnaryExpression unaryExpression
                ? (MemberExpression)unaryExpression.Operand
                : (MemberExpression)expression.Body;

            return memberExpression.Member;
        }

        public PrintingConfig<TOwner> Trim(Expression<Func<TOwner, string>> property, int length)
        {
            Func<string, string> finalFunc = value => TrimString(value, length);

            var memberInfo = GetMemberInfo(property);
            if (membersSerializesInfos.TryGetValue(memberInfo, out var serialize))
                finalFunc = value => TrimString(serialize(value), length);

            return Using(property, finalFunc);
        }

        public PrintingConfig<TOwner> Trim(int length)
        {
            Func<string, string> finalFunc = value => TrimString(value, length);

            if (typeSerializesInfos.TryGetValue(typeof(string), out var serialize))
                finalFunc = value => TrimString(serialize(value), length);

            return Using(finalFunc);
        }

        private string TrimString(string value, int length)
        {
            return value.Length <= length
                ? value
                : value[..length];
        }

        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));

            return this;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            var serializer = new Serializer(
                excludedMembers,
                excludedTypes,
                membersSerializesInfos,
                typeSerializesInfos,
                maxRecursion);

            return serializer.Serialize(obj, nestingLevel);
        }
    }
}