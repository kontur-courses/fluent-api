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
        private readonly HashSet<string> membersToExclude = new HashSet<string>();

        private readonly HashSet<Type> typesToExclude = new HashSet<Type>();

        public MemberPrintingConfig<TOwner, TMemberType> Printing<TMemberType>()
        {
            //typesToExclude.Add(typeof(TMemberType));
            return new MemberPrintingConfig<TOwner, TMemberType>(this);
        }

        public MemberPrintingConfig<TOwner, TMemberType> Printing<TMemberType>(
            Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            var memberInfo = ((MemberExpression)memberSelector.Body).Member;
            membersToExclude.Add(memberInfo.Name);

            return new MemberPrintingConfig<TOwner, TMemberType>(this);
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

                var memberValue = PrintToString(member.GetValue(obj), nestingLvl + 1);
                builder.AppendNextMember(member, memberValue, GetIndent(nestingLvl));
            }

            return builder.ToString();
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