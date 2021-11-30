using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Extensions;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly Dictionary<MemberInfo, Delegate> alternativeMemberSerializers = new();
        private readonly Dictionary<Type, Delegate> alternativeTypeSerializers = new();
        private readonly HashSet<MemberInfo> excludingMembers = new();
        private readonly HashSet<Type> excludingTypes = new();

        private readonly HashSet<Type> finalTypes = new()
        {
            typeof(int),
            typeof(double),
            typeof(float),
            typeof(string),
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(Guid)
        };

        private readonly Dictionary<object, int> visitedMembers = new();

        public void AddAlternativeTypeSerializer<TPropType>(Type type, Func<TPropType, string> serializer)
        {
            alternativeTypeSerializers.TryAdd(type, serializer);
        }

        public void AddAlternativeMemberSerializer<TPropType>(MemberInfo memberInfo, Func<TPropType, string> serializer)
        {
            alternativeMemberSerializers.TryAdd(memberInfo, serializer);
        }

        public TypePrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, GetMemberInfo(memberSelector));
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludingTypes.Add(typeof(TPropType));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberInfo = GetMemberInfo(memberSelector);
            excludingMembers.Add(memberInfo);
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (visitedMembers.TryGetValue(obj, out var level) && nestingLevel > level)
                return "Обнаружена циклическая ссылка";

            visitedMembers.Add(obj, nestingLevel);

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            if (obj is IEnumerable collection)
            {
                var builder = new StringBuilder($"{obj.GetType().Name}{Environment.NewLine}");
                foreach (var member in collection)
                {
                    builder.Append(member.PrintToString());
                }

                return builder.ToString();
            }

            var type = obj.GetType();
            sb.AppendLine(type.Name);

            var members = type.GetProperties().Concat<MemberInfo>(type.GetFields());
            foreach (var memberInfo in members
                .Where(x => !excludingTypes.Contains(x.GetMemberType()))
                .Where(x => !excludingMembers.Contains(x)))
            {
                var serialized = TryGetAlternativeSerializer(memberInfo, out var serializer) 
                    ? serializer.DynamicInvoke(memberInfo.GetValue(obj)) + Environment.NewLine
                    : PrintToString(memberInfo.GetValue(obj), nestingLevel + 1);
                sb.Append(identation + memberInfo.Name + " = " + serialized);
            }

            return sb.ToString();
        }

        private static MemberInfo GetMemberInfo<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberExpression = memberSelector.Body as MemberExpression;
            return memberExpression?.Member;
        }

        private bool TryGetAlternativeSerializer(MemberInfo memberInfo, out Delegate serializer)
        {
            if (alternativeMemberSerializers.TryGetValue(memberInfo, out serializer))
                return true;

            var memberType = memberInfo.GetMemberType();
            if (alternativeTypeSerializers.TryGetValue(memberType, out serializer))
                return true;

            serializer = null;
            return false;
        }
    }
}