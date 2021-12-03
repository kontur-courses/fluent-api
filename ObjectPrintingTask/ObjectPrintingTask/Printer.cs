using ObjectPrintingTask.Extensions;
using ObjectPrintingTask.PrintingConfiguration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ObjectPrintingTask
{
    public class Printer<TOwner>
    {
        private PrintingConfig<TOwner> config;
        private readonly HashSet<object> visitedObjects = new HashSet<object>();

        public Printer(PrintingConfig<TOwner> config)
        {
            this.config = config;
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

            if (IsSimple(type))
                return obj.ToString();

            if (visitedObjects.Contains(obj))
                return "[cyclic reference detected]";

            visitedObjects.Add(obj);

            return obj switch
            {
                IDictionary => GetItemsFromDictionary((IDictionary)obj, nestingLevel),
                ICollection => GetItemsFromCollection((ICollection)obj, nestingLevel),
                _ => PrintMembersOfObject(obj, type, nestingLevel)
            };
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
            return config.ShouldExcludeMemberByName(memberInfo.GetFullName())
                   || config.ShouldExcludeMemberByType(memberInfo.GetMemberInstanceType());
        }

        private string GetMemberValue(object obj, MemberInfo member, int nestingLevel)
        {
            var hasAlternateScenario = config.IsMemberHasAlternateScenario(member.GetFullName())
                || config.IsMemberTypeHasAlternateScenario(member.GetMemberInstanceType());

            Delegate scenario = null;
            if (config.IsMemberTypeHasAlternateScenario(member.GetMemberInstanceType()))
                scenario = config.GetMemberTypeScenario(member.GetMemberInstanceType());

            if (config.IsMemberHasAlternateScenario(member.GetFullName()))
                scenario = config.GetMemberScenario(member.GetFullName());

            return hasAlternateScenario
                ? (string)scenario.DynamicInvoke(member.GetValue(obj))
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

        private static string GetIndent(int nestingLevel)
        {
            return new string('\t', nestingLevel + 1);
        }

        bool IsSimple(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // nullable type, check if the nested type is simple.
                return IsSimple(typeInfo.GetGenericArguments()[0]);
            }
            return typeInfo.IsPrimitive
              || typeInfo.IsEnum
              || typeInfo.IsValueType
              || type.Equals(typeof(string))
              || type.Equals(typeof(decimal));
        }
    }
}
