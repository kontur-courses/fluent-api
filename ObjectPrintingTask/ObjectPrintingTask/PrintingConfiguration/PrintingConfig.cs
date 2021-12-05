using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ObjectPrintingTask.Extensions;

namespace ObjectPrintingTask.PrintingConfiguration
{
    public class PrintingConfig<TOwner>
    {
        private readonly Dictionary<string, Delegate> memberScenarios = new Dictionary<string, Delegate>();
        private readonly HashSet<string> membersToExclude = new HashSet<string>();
        private readonly Dictionary<Type, Delegate> typeScenarios = new Dictionary<Type, Delegate>();
        private readonly HashSet<Type> typesToExclude = new HashSet<Type>();

        public PrintingConfig<TOwner> Excluding<TMemberType>(
            Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            if (memberSelector == null)
                throw new ArgumentNullException($"Member selector {memberSelector} can not be null");

            var memberFullName = ((MemberExpression)memberSelector.Body).Member.GetFullName();
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

        public bool ShouldExcludeMemberByType(Type memberType)
        {
            return typesToExclude.Contains(memberType);
        }

        public bool ShouldExcludeMemberByName(string memberFullName)
        {
            return membersToExclude.Contains(memberFullName);
        } 

        public bool IsMemberHasAlternateScenario(string memberFullName)
        {      
            return memberScenarios.ContainsKey(memberFullName);
        }

        public bool IsMemberTypeHasAlternateScenario(Type memberType)
        {
            return typeScenarios.ContainsKey(memberType);
        }  

        public Delegate GetMemberScenario(string memberFullName)
        {
            return memberScenarios[memberFullName];
        }

        public Delegate GetMemberTypeScenario(Type memberType)
        {
            return typeScenarios[memberType];
        }
    }
}