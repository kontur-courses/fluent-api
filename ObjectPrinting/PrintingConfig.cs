using ObjectPrinting.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly Dictionary<Type, Delegate> alternativeTypeSerializators = new();
        private readonly Dictionary<MemberInfo, Delegate> alternativeMemberSerializators = new();
        private readonly Dictionary<object, int> visitedMembers = new();
        private readonly HashSet<Type> excludingTypes = new();
        private readonly HashSet<MemberInfo> excludingMembers = new();
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

        public void AddAlternativeTypeSerializator<TPropType>(Type type, Func<TPropType, string> serializator)
        {
            alternativeTypeSerializators.TryAdd(type, serializator);
        }

        public void AddAlternativeMemberSerializator<TPropType>(MemberInfo memberInfo, Func<TPropType, string> serializator)
        {
            alternativeMemberSerializators.TryAdd(memberInfo, serializator);
        }

        public TypePrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
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

            if (visitedMembers.TryGetValue(obj, out int level) && nestingLevel > level)
                return $"Обнаружена циклическая ссылка";

            visitedMembers.Add(obj, nestingLevel);

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            var members = type.GetProperties().Concat<MemberInfo>(type.GetFields());
            foreach (var memberInfo in members
                .Where(x => !excludingTypes.Contains(x.GetMemberType()))
                .Where(x => !excludingMembers.Contains(x)))
            {
                TryGetAlternativeSerializator(memberInfo, out Delegate serializator);
                var serialized = serializator?.DynamicInvoke(memberInfo.GetValue(obj))
                    ?? PrintToString(memberInfo.GetValue(obj), nestingLevel + 1);
                sb.Append(identation + memberInfo.Name + " = " + serialized);
            }
            return sb.ToString();
        }

        private static MemberInfo GetMemberInfo<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberExpression = memberSelector.Body as MemberExpression;
            return memberExpression?.Member;
        }

        private bool TryGetAlternativeSerializator(MemberInfo memberInfo, out Delegate serializator)
        {

            if (alternativeMemberSerializators.TryGetValue(memberInfo, out serializator))
                return true;

            var memberType = memberInfo.GetType();
            if (alternativeTypeSerializators.TryGetValue(memberType, out serializator))
                return true;

            serializator = null;
            return false;
        }
    }
}