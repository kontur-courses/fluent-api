using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Extensions;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float),
            typeof(bool), typeof(long), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        private readonly HashSet<MemberInfo> excludedMembers = new HashSet<MemberInfo>();
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<object> serializedObjects = new HashSet<object>();

        private readonly Dictionary<MemberInfo, Func<object, string>> memberSerializers =
            new Dictionary<MemberInfo, Func<object, string>>();

        public string PrintToString(TOwner obj) => PrintToString(obj, 0);

        public PrintingConfig<TOwner> ExcludeProperty<TProperty>()
        {
            excludedTypes.Add(typeof(TProperty));
            return this;
        }

        public PrintingConfig<TOwner> ExcludeProperty<TProperty>(Expression<Func<TOwner, TProperty>> memberSelector)
        {
            excludedMembers.Add(((MemberExpression)memberSelector.Body).Member);
            return this;
        }

        public PrintingPropertyConfig<TOwner, TProperty> ChangeSerializationFor<TProperty>() =>
            new PrintingPropertyConfig<TOwner, TProperty>(this, memberSerializers, typeof(TProperty));

        public PrintingPropertyConfig<TOwner, TPropType> ChangeSerializationFor<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var body = (MemberExpression)memberSelector.Body;
            return new PrintingPropertyConfig<TOwner, TPropType>(this, memberSerializers, body.Member);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (finalTypes.Contains(obj.GetType()))
            {
                if (memberSerializers.TryGetValue(obj.GetType(), out var serializer))
                    return serializer(obj) + Environment.NewLine;
                return obj + Environment.NewLine;
            }

            if (!serializedObjects.Add(obj))
                return "Cycle, object was already serialized." + Environment.NewLine;

            return obj switch
            {
                IList collection => SerializeCollection(collection, nestingLevel),
                IDictionary dictionary => SerializeDictionary(dictionary, nestingLevel),
                _ => SerializeObject(obj, nestingLevel)
            };
        }
        
        private string SerializeCollection(IList collection, int nestingLevel)
        {
            var sb = new StringBuilder(collection.GetType().Name + " {" + Environment.NewLine);
            foreach (var item in collection)
            {
                sb.Append(new string('\t', nestingLevel + 1));
                sb.Append(PrintToString(item, nestingLevel + 1));
            }
            sb.Append(new string('\t', nestingLevel) + '}' + Environment.NewLine);
            return sb.ToString();
        }
        
        private string SerializeDictionary(IDictionary dictionary, int nestingLevel)
        {
            var sb = new StringBuilder(dictionary.GetType().Name + " {" + Environment.NewLine);
            foreach (var key in dictionary.Keys)
            {
                sb.Append(new string('\t', nestingLevel + 1));
                sb.Append(PrintToString(key, nestingLevel + 1).Trim() + " = ");
                sb.Append(PrintToString(dictionary[key], nestingLevel+ 1));
            }
            sb.Append(new string('\t', nestingLevel) + '}' + Environment.NewLine);

            return sb.ToString();
        }

        private string SerializeObject(object obj, int nestingLevel)
        {
            var indent = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var memberInfo in type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                         .Where(t => t.MemberType == MemberTypes.Field || t.MemberType == MemberTypes.Property)
                         .Where(x => !IsMemberExcluded(x)))
            {
                sb.Append(
                    indent + memberInfo.Name + " = " +
                    SerializeMember(obj, memberInfo, nestingLevel + 1)
                );
            }

            return sb.ToString();
        }

        private string SerializeMember(object obj, MemberInfo memberInfo, int nestingLevel)
        {
            return TryGetCustomSerializer(memberInfo, out var serializer)
                ? serializer.Invoke(memberInfo.GetValue(obj)) + Environment.NewLine
                : PrintToString(memberInfo.GetValue(obj), nestingLevel);
        }

        private bool TryGetCustomSerializer(MemberInfo memberInfo, out Func<object, string> serializer)
        {
            return memberSerializers.TryGetValue(memberInfo, out serializer) ||
                   memberSerializers.TryGetValue(memberInfo.GetMemberType(), out serializer);
        }

        private bool IsMemberExcluded(MemberInfo memberInfo)
        {
            return excludedMembers.Contains(memberInfo) ||
                   excludedTypes.Contains(memberInfo.GetMemberType());
        }
    }
}
