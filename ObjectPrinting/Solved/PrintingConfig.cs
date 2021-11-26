using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting.Solved
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

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (IsObjectFinalType(obj))
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var builder = new StringBuilder();
            var type = obj.GetType();

            builder.AppendLine(type.Name);

            foreach (var memberInfo in type.GetPropertiesAndFields())
            {
                if (!memberInfo.IsFieldOrProperty())
                    continue;

                if (ShouldExcludeMemberByType(memberInfo.GetMemberInstanceType()))
                    continue;

                if (ShouldExcludeMemberByName(memberInfo.Name))
                    continue;

                builder
                    .Append(indentation)
                    .Append(memberInfo.Name)
                    .Append(" = ")
                    .Append(PrintToString(memberInfo.GetValue(obj), nestingLevel + 1))
                    .Append("\r\n");
            }

            return builder.ToString();
        }

        private bool ShouldExcludeMemberByName(string memberName)
        {
            return membersToExclude.Contains(memberName);
        }

        private bool IsObjectFinalType(object obj)
        {
            return finalTypes.Contains(obj.GetType());
        }

        private bool ShouldExcludeMemberByType(Type memberType)
        {
            return typesToExclude.Contains(memberType);
        }
    }
}