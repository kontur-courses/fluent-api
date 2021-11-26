using System;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Solved.Extensions
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder AppendNextMember(
            this StringBuilder builder,
            MemberInfo member,
            string memberValue,
            string indent = "")
        {
            return builder
                .Append(indent)
                .Append(member.Name)
                .Append(" = ")
                .Append(memberValue)
                .Append(Environment.NewLine);
        }
    }
}