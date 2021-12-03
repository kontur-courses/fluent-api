using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrintingTask.Extensions;

namespace ObjectPrintingTask.PrintingConfiguration
{
    public class PrintingConfig<TOwner>
    {
        private readonly Dictionary<string, Delegate> memberScenarios = new Dictionary<string, Delegate>();
        private readonly HashSet<string> membersToExclude = new HashSet<string>();
        private readonly Dictionary<Type, Delegate> typeScenarios = new Dictionary<Type, Delegate>();
        private readonly HashSet<Type> typesToExclude = new HashSet<Type>();
        private readonly HashSet<object> visitedObjects = new HashSet<object>();

        public TypePrintingConfig<TOwner, TMemberType> PrintingType<TMemberType>()
        {
            return new TypePrintingConfig<TOwner, TMemberType>(this, typeof(TMemberType));
        }

        public MemberPrintingConfig<TOwner, TMemberType> PrintingMember<TMemberType>(
            Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            var member = ((MemberExpression)memberSelector.Body).Member;
            var memberFullName = GetMemberFullName(member);
            return new MemberPrintingConfig<TOwner, TMemberType>(this, memberFullName);
        }

        public PrintingConfig<TOwner> Excluding<TMemberType>(
            Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            var member = ((MemberExpression)memberSelector.Body).Member;
            var memberFullName = GetMemberFullName(member);
            membersToExclude.Add(memberFullName);

            return this;
        }

        public PrintingConfig<TOwner> Excluding<TMemberType>()
        {
            typesToExclude.Add(typeof(TMemberType));
            return this;
        }

        public void AddSerializingScenario<TMemberType>(string memberFullName, Func<TMemberType, string> scenario)
        {
            memberScenarios[memberFullName] = scenario;
        }

        public void AddSerializingScenario<TMemberType>(Type type, Func<TMemberType, string> scenario)
        {
            typeScenarios[type] = scenario;
        }

        public string PrintToString(TOwner obj)
        {
            visitedObjects.Clear();
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null";

            var type = obj.GetType();
            
            if (type.IsPrimitive || FinalTypesKeeper.IsFinalType(type))
                return obj.ToString();

            if (visitedObjects.Contains(obj))
                return "[cyclic reference detected]";

            visitedObjects.Add(obj);

            if (type.GetInterface(nameof(ICollection)) != null)
                return type.GetInterface(nameof(IDictionary)) == null
                    ? GetItemsFromCollection((ICollection)obj, nestingLevel)
                    : GetItemsFromDictionary((IDictionary)obj, nestingLevel);

            return PrintMembersOfObject(obj, type, nestingLevel);
        }

        private string PrintMembersOfObject(object obj, Type type, int nestingLevel)
        {
            var builder = new StringBuilder();
            builder.AppendLine(type.Name);

            foreach (var member in type.GetPropertiesAndFields())
            {
                if (ShouldIgnoreMember(member))
                    continue;

                builder
                .Append(GetIndent(nestingLevel))
                .Append(member.Name)
                .Append(" = ")
                .Append(GetMemberValue(obj, member, nestingLevel))
                .Append(Environment.NewLine);
            }

            return builder.ToString();
        }

        private bool ShouldIgnoreMember(MemberInfo memberInfo)
        {
            return ShouldExcludeMemberByName(string.Join(".", memberInfo.ReflectedType.Name, memberInfo.Name))
                   || ShouldExcludeMemberByType(memberInfo.GetMemberInstanceType());
        }

        private bool ShouldExcludeMemberByType(Type memberType)
        {
            return typesToExclude.Contains(memberType);
        }

        private bool ShouldExcludeMemberByName(string memberName)
        {
            return membersToExclude.Contains(memberName);
        }

        private string GetMemberValue(object obj, MemberInfo member, int nestingLevel)
        {
            return IsMemberHasAlternateScenario(member) || IsMemberTypeHasAlternateScenario(member)
                ? GetValueFromScenario(obj, member)
                : PrintToString(member.GetValue(obj), nestingLevel + 1);
        }

        private string GetItemsFromDictionary(IDictionary dictionary, int nestingLevel)
        {
            var builder = new StringBuilder();
            builder.Append("[");

            foreach (var key in dictionary.Keys)
            {
                var keyToString = PrintToString(key, nestingLevel);
                var valueToString = PrintToString(dictionary[key], nestingLevel);
                builder
                .Append(Environment.NewLine)
                .Append(GetIndent(nestingLevel))
                .Append(key)
                .Append(" : ")
                .Append(valueToString);
            }

            builder.Append(Environment.NewLine).Append("]").Append(Environment.NewLine);

            return builder.ToString();
        }

        private string GetItemsFromCollection(ICollection collection, int nestingLevel)
        {
            var builder = new StringBuilder();
            builder.Append("[");

            foreach (var item in collection)
                builder.Append(PrintToString(item, nestingLevel + 1)).Append(", ");

            builder.Remove(builder.Length - 2, 2).Append("]").Append(Environment.NewLine);

            return builder.ToString();
        }

        private bool IsMemberHasAlternateScenario(MemberInfo member)
        {
            var memberFullName = GetMemberFullName(member);
            return memberScenarios.ContainsKey(memberFullName);
        }

        private bool IsMemberTypeHasAlternateScenario(MemberInfo member)
        {
            return typeScenarios.ContainsKey(member.GetMemberInstanceType());
        }

        private string GetValueFromScenario(object obj, MemberInfo member)
        {
            var memberFullName = GetMemberFullName(member);

            var scenario = IsMemberHasAlternateScenario(member)
                ? memberScenarios[memberFullName]
                : typeScenarios[member.GetMemberInstanceType()];

            return (string)scenario.DynamicInvoke(member.GetValue(obj));
        }

        private static string GetIndent(int nestingLevel)
        {
            return new string('\t', nestingLevel + 1);
        }

        private string GetMemberFullName(MemberInfo member)
        {
            return string.Join(".", member.ReflectedType.Name, member.Name);
        }
    }
}