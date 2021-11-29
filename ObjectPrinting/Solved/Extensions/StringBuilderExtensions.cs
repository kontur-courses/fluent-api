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

        public static StringBuilder AppendKeyValuePair(
            this StringBuilder builder,
            string indent, string key, string value,
            string keyValueDelimiter = " : ")
        {
            return builder
                .Append(Environment.NewLine)
                .Append(indent)
                .Append(key)
                .Append(keyValueDelimiter)
                .Append(value);
        }
    }
}