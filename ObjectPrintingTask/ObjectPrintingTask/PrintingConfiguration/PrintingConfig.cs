using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using ObjectPrintingTask.Extensions;

namespace ObjectPrintingTask.PrintingConfiguration
{
    public class PrintingConfig<TOwner>
    {
        private readonly Dictionary<MemberInfo, Delegate> memberScenarios = new Dictionary<MemberInfo, Delegate>();
        private readonly HashSet<MemberInfo> membersToExclude = new HashSet<MemberInfo>();
        private readonly Dictionary<Type, Delegate> typeScenarios = new Dictionary<Type, Delegate>();
        private readonly HashSet<Type> typesToExclude = new HashSet<Type>();

        public PrintingConfig<TOwner> Excluding<TMemberType>(
            Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            if (memberSelector == null)
                throw new ArgumentNullException(nameof(memberSelector), "Member selector can not be null");

            var member = ((MemberExpression)memberSelector.Body).Member;
            membersToExclude.Add(member);

            return this;
        }

        public PrintingConfig<TOwner> Excluding<TMemberType>()
        {
            typesToExclude.Add(typeof(TMemberType));
            return this;
        }

        public void AddSerializingScenario<TMemberType>(MemberInfo member, Func<TMemberType, string> scenario)
        {
            memberScenarios[member] = scenario;
        }

        public void AddSerializingScenario<TMemberType>(Type type, Func<TMemberType, string> scenario)
        {
            typeScenarios[type] = scenario;
        }

        public bool ShouldExcludeMemberByType(Type memberType)
        {
            return typesToExclude.Contains(memberType);
        }

        public bool ShouldExcludeMemberByName(MemberInfo member)
        {
            return membersToExclude.Contains(member);
        } 

        public bool IsMemberHasAlternateScenario(MemberInfo member)
        {      
            return memberScenarios.ContainsKey(member);
        }

        public bool IsMemberTypeHasAlternateScenario(Type memberType)
        {
            return typeScenarios.ContainsKey(memberType);
        }  

        public Delegate GetMemberScenario(MemberInfo member)
        {
            return memberScenarios[member];
        }

        public Delegate GetMemberTypeScenario(Type memberType)
        {
            return typeScenarios[memberType];
        }
    }
}