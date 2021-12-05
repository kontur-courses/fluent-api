using ObjectPrintingTask.Extensions;
using ObjectPrintingTask.PrintingConfiguration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrintingTask
{
    public class Printer<TOwner>
    {
        private PrintingConfig<TOwner> config;
        private Dictionary<Type, IEnumerable<MemberInfo>> cachedMembers = new Dictionary<Type, IEnumerable<MemberInfo>>();
        private readonly HashSet<object> visitedObjects = new HashSet<object>();

        public Printer(PrintingConfig<TOwner> config)
        {
            this.config = config;
        }

        public TypePrintingConfig<TOwner, TMemberType> PrintingType<TMemberType>()
        {
            return new TypePrintingConfig<TOwner, TMemberType>(this, typeof(TMemberType));
        }

        public MemberPrintingConfig<TOwner, TMemberType> PrintingMember<TMemberType>(
            Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            if (memberSelector == null)
                throw new ArgumentNullException(nameof(memberSelector), "Member selector can not be null");

            var member = ((MemberExpression)memberSelector.Body).Member;

            return new MemberPrintingConfig<TOwner, TMemberType>(this, member);
        }

        public Printer<TOwner> Excluding<TMemberType>(
            Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            config.Excluding(memberSelector);
            return this;
        }

        public Printer<TOwner> Excluding<TMemberType>()
        {
            config.Excluding<TMemberType>();
            return this;
        }

        public void AddSerializingScenario<TMemberType>(MemberInfo member, Func<TMemberType, string> scenario)
        {
            config.AddSerializingScenario(member, scenario);
        }

        public void AddSerializingScenario<TMemberType>(Type type, Func<TMemberType, string> scenario)
        {
            config.AddSerializingScenario(type, scenario);
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
            var objToString = obj switch
            {
                IDictionary => GetItemsFromDictionary((IDictionary)obj, nestingLevel),
                IEnumerable => GetItemsFromEnumerable((IEnumerable)obj, nestingLevel),
                _ => PrintMembersOfObject(obj, type, nestingLevel)
            };

            visitedObjects.Remove(obj);
            return objToString;
        }

        private string PrintMembersOfObject(object obj, Type type, int nestingLevel)
        {
            var builder = new StringBuilder();
            builder.AppendLine(type.Name);

            if (!cachedMembers.ContainsKey(type))
                cachedMembers.Add(type, type.GetPropertiesAndFields());
            

            foreach (var member in cachedMembers[type])
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

        private bool ShouldIgnoreMember(MemberInfo member)
        {
            return config.ShouldExcludeMemberByName(member)
                   || config.ShouldExcludeMemberByType(member.GetMemberInstanceType())                 ;
        }

        private string GetMemberValue(object obj, MemberInfo member, int nestingLevel)
        {
            var hasAlternateScenario = config.IsMemberHasAlternateScenario(member)
                || config.IsMemberTypeHasAlternateScenario(member.GetMemberInstanceType());

            Delegate scenario = null;
            if (config.IsMemberTypeHasAlternateScenario(member.GetMemberInstanceType()))
                scenario = config.GetMemberTypeScenario(member.GetMemberInstanceType());

            if (config.IsMemberHasAlternateScenario(member))
                scenario = config.GetMemberScenario(member);

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

        private string GetItemsFromEnumerable(IEnumerable collection, int nestingLevel)
        {           
            var builder = new StringBuilder();
            builder.Append("[");

            var itemsCount = 0;
            foreach (var item in collection)
            {
                builder.Append(PrintToString(item, nestingLevel + 1)).Append(", ");
                itemsCount++;
            }

            if (itemsCount > 0)
                builder.Remove(builder.Length - 2, 2);

            builder.Append("]").Append(Environment.NewLine);
            return builder.ToString();
        }

        private static string GetIndent(int nestingLevel)
        {
            return new string('\t', nestingLevel + 1);
        }

        private bool IsSimple(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return IsSimple(typeInfo.GetGenericArguments()[0]);
            }

            //if (!typeInfo.IsPrimitive && typeInfo.IsValueType)
            //    return false;

            return typeInfo.IsPrimitive
              || typeInfo.IsEnum
              || type.Equals(typeof(DateTime))
              || type.Equals(typeof(TimeSpan))
              || type.Equals(typeof(Guid))
              || type.Equals(typeof(string))
              || type.Equals(typeof(decimal));
        }
    }
}
