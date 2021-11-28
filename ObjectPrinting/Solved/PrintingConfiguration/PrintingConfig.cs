using System;
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

        public void AddSerializingScenario<TMemberType>(string memberName, Func<TMemberType, string> scenario)
        {
            memberScenarios[memberName] = scenario;
        }

        public void AddSerializingScenario<TMemberType>(Type type, Func<TMemberType, string> scenario)
        {
            typeScenarios[type] = scenario;
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

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLvl)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (FinalTypesKeeper.IsFinalType(obj.GetType()))
                return obj + Environment.NewLine;

            var builder = new StringBuilder();
            var type = obj.GetType();

            builder.AppendLine(type.Name);

            foreach (var member in type.GetPropertiesAndFields())
            {
                if (ShouldIgnoreMember(member))
                    continue;

                var value = GetMemberValue(obj, member, nestingLvl);
                builder.AppendNextMember(member, value, GetIndent(nestingLvl));
            }

            return builder.ToString();
        }

        private string GetMemberValue(object obj, MemberInfo member, int nestingLvl)
        {
            return IsMemberHasAlternateScenario(member) || IsMemberTypeHasAlternateScenario(member)
                ? GetValueFromScenario(obj, member)
                : PrintToString(member.GetValue(obj), nestingLvl + 1);
        }

        private string GetValueFromScenario(object obj, MemberInfo member)
        {
            var scenario = IsMemberHasAlternateScenario(member)
                ? memberScenarios[member.Name]
                : typeScenarios[member.GetMemberInstanceType()];

            return (string)scenario.DynamicInvoke(member.GetValue(obj));
        }

        private bool IsMemberHasAlternateScenario(MemberInfo member)
        {
            return memberScenarios.ContainsKey(member.Name);
        }

        private bool IsMemberTypeHasAlternateScenario(MemberInfo member)
        {
            return typeScenarios.ContainsKey(member.GetMemberInstanceType());
        }

        private bool ShouldIgnoreMember(MemberInfo memberInfo)
        {
            if (!memberInfo.IsFieldOrProperty())
                return true;

            if (ShouldExcludeMemberByType(memberInfo.GetMemberInstanceType()))
                return true;

            if (ShouldExcludeMemberByName(memberInfo.Name))
                return true;

            return false;
        }

        private bool ShouldExcludeMemberByType(Type memberType)
        {
            return typesToExclude.Contains(memberType);
        }

        private bool ShouldExcludeMemberByName(string memberName)
        {
            return membersToExclude.Contains(memberName);
        }

        private static string GetIndent(int nestingLvl)
        {
            return new string('\t', nestingLvl + 1);
        }
    }
}