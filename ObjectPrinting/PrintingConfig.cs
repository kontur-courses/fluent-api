using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly ExcludingConfig excludingConfig = new ExcludingConfig();
        
        private readonly Dictionary<MemberInfo, Delegate> specializedMemberSerializers =
            new Dictionary<MemberInfo, Delegate>();

        private readonly HashSet<object> visited = new HashSet<object>();

        private readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        public MemberPrintingConfig<TOwner, TMemberType> Printing<TMemberType>()
        {
            return new MemberPrintingConfig<TOwner, TMemberType>(this);
        }

        public MemberPrintingConfig<TOwner, TMemberType> Printing<TMemberType>(
            Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            if (memberSelector.Body is MemberExpression memberExpression)
            {
                var member = memberExpression.Member;
                return new MemberPrintingConfig<TOwner, TMemberType>(this, member);
            }

            throw new InvalidOperationException();
        }

        public PrintingConfig<TOwner> Excluding<TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
        {
            if (memberSelector.Body is MemberExpression memberExpression)
            {
                var excludedMember = memberExpression.Member;
                excludingConfig.Exclude(excludedMember);
            }

            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludingConfig.Exclude<TPropType>();
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public PrintingConfig<TOwner> SpecializeSerialization(MemberInfo member, Delegate serializer)
        {
            specializedMemberSerializers.Add(member, serializer);
            return this;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            visited.Add(obj);

            var builder = new StringBuilder();

            var type = obj.GetType();
            builder.AppendLine(type.Name);

            if (obj is IEnumerable collection)
                SerializeCollection(builder, collection, nestingLevel);
            else
                SerializeMembers(builder, obj, nestingLevel);


            visited.Remove(obj);

            return builder.ToString();
        }

        private void SerializeCollection(StringBuilder builder, IEnumerable collection, int nestingLevel)
        {
            var indentation = GetIndentation(nestingLevel);

            foreach (var element in collection)
            {
                builder
                    .Append(indentation)
                    .Append(PrintToString(element, nestingLevel + 1));
            }
        }

        private void SerializeMembers(StringBuilder builder, object obj, int nestingLevel)
        {
            var indentation = GetIndentation(nestingLevel);
            var type = obj.GetType();

            foreach (var memberInfo in type.GetFieldsAndProperties())
            {
                Func<object, string> serializationFunc = GetSerializationFunction(memberInfo, nestingLevel);

                if (!MemberIsExcluded(memberInfo))
                {
                    var objj = memberInfo.GetValue(obj);
                    if (visited.Contains(objj)) continue;
                    builder
                        .Append(indentation)
                        .Append(memberInfo.Name)
                        .Append(" = ")
                        .Append(serializationFunc(objj));
                }
            }
        }

        private Func<object, string> GetSerializationFunction(MemberInfo memberInfo, int nestingLevel)
        {
            if (specializedMemberSerializers.TryGetValue(memberInfo, out var serializer))
                return x => serializer.DynamicInvoke(x) + Environment.NewLine;
            if (specializedMemberSerializers.TryGetValue(memberInfo.GetReturnType(), out serializer))
                return x => serializer.DynamicInvoke(x) + Environment.NewLine;
            return x => PrintToString(x, nestingLevel + 1);
        }

        private bool MemberIsExcluded(MemberInfo member)
        {
            return excludingConfig.IsExcluded(member);
        }

        private string GetIndentation(int nestingLevel)
        {
            return new string('\t', nestingLevel + 1);
        }
    }
}