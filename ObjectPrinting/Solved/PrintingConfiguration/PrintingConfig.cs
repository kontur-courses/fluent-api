using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Solved.Extensions;

namespace ObjectPrinting.Solved.PrintingConfiguration
{
    public class PrintingConfig<TOwner>
    {
        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        private readonly HashSet<string> membersToExclude = new HashSet<string>();

        private readonly HashSet<Type> typesToExclude = new HashSet<Type>();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberInfo = ((MemberExpression)memberSelector.Body).Member;
            membersToExclude.Add(memberInfo.Name);

            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            typesToExclude.Add(typeof(TPropType));
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

            if (IsObjectFinalType(obj))
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

        private bool IsObjectFinalType(object obj)
        {
            return finalTypes.Contains(obj.GetType());
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

        private string GetIndent(int nestingLvl)
        {
            return new string('\t', nestingLvl + 1);
        }
    }
}