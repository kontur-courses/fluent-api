using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Solved.Extensions;

namespace ObjectPrinting.Solved.PrintingConfiguration
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
            var memberName = ((MemberExpression)memberSelector.Body).Member.Name;
            return new MemberPrintingConfig<TOwner, TMemberType>(this, memberName);
        }

        public PrintingConfig<TOwner> Excluding<TMemberType>(
            Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            var memberInfo = ((MemberExpression)memberSelector.Body).Member;
            membersToExclude.Add(memberInfo.Name);

            return this;
        }

        public PrintingConfig<TOwner> Excluding<TMemberType>()
        {
            typesToExclude.Add(typeof(TMemberType));
            return this;
        }

        public void AddSerializingScenario<TMemberType>(string memberName, Func<TMemberType, string> scenario)
        {
            memberScenarios[memberName] = scenario;
        }

        public void AddSerializingScenario<TMemberType>(Type type, Func<TMemberType, string> scenario)
        {
            typeScenarios[type] = scenario;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLvl)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (FinalTypesKeeper.IsFinalType(obj.GetType()))
                return obj.ToString();

            if (visitedObjects.Contains(obj))
                return "[cyclic reference detected]";

            visitedObjects.Add(obj);

            var type = obj.GetType();

            if (type.GetInterface(nameof(ICollection)) != null)
                return type.GetInterface(nameof(IDictionary)) == null
                    ? GetItemsFromCollection((ICollection)obj, nestingLvl)
                    : GetItemsFromDictionary((IDictionary)obj, nestingLvl);

            return PrintMembersOfObject(obj, type, nestingLvl);
        }

        private string PrintMembersOfObject(object obj, Type type, int nestingLvl)
        {
            var builder = new StringBuilder();
            builder.AppendLine(type.Name);

            foreach (var member in type.GetPropertiesAndFields())
            {
                if (ShouldIgnoreMember(member))
                    continue;

                builder.AppendNextMember(member, GetMemberValue(obj, member, nestingLvl), GetIndent(nestingLvl));
            }

            return builder.ToString();
        }

        private bool ShouldIgnoreMember(MemberInfo memberInfo)
        {
            return ShouldExcludeMemberByName(memberInfo.Name)
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

        private string GetMemberValue(object obj, MemberInfo member, int nestingLvl)
        {
            return IsMemberHasAlternateScenario(member) || IsMemberTypeHasAlternateScenario(member)
                ? GetValueFromScenario(obj, member)
                : PrintToString(member.GetValue(obj), nestingLvl + 1);
        }

        private string GetItemsFromDictionary(IDictionary dictionary, int nestingLvl)
        {
            var builder = new StringBuilder();
            builder.Append("[");

            foreach (var key in dictionary.Keys)
            {
                var keyToString = PrintToString(key, nestingLvl + 1);
                var valueToString = PrintToString(dictionary[key], nestingLvl + 1);
                builder.AppendKeyValuePair(GetIndent(nestingLvl + 1), keyToString, valueToString);
            }

            builder.Append(Environment.NewLine).Append(GetIndent(nestingLvl)).Append("]");

            return builder.ToString();
        }

        private string GetItemsFromCollection(ICollection collection, int nestingLvl)
        {
            var builder = new StringBuilder();
            builder.Append("[");

            foreach (var item in collection)
                builder.Append(PrintToString(item, nestingLvl + 1)).Append(", ");

            builder.Remove(builder.Length - 2, 2).Append("]");

            return builder.ToString();
        }

        private bool IsMemberHasAlternateScenario(MemberInfo member)
        {
            return memberScenarios.ContainsKey(member.Name);
        }

        private bool IsMemberTypeHasAlternateScenario(MemberInfo member)
        {
            return typeScenarios.ContainsKey(member.GetMemberInstanceType());
        }

        private string GetValueFromScenario(object obj, MemberInfo member)
        {
            var scenario = IsMemberHasAlternateScenario(member)
                ? memberScenarios[member.Name]
                : typeScenarios[member.GetMemberInstanceType()];

            return (string)scenario.DynamicInvoke(member.GetValue(obj));
        }

        private static string GetIndent(int nestingLvl)
        {
            return new string('\t', nestingLvl + 1);
        }
    }
}