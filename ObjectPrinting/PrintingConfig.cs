using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using ObjectPrinting.Extensions;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
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
            var objType = obj.GetType();
            if (objType.IsPrimitive || objType.IsValueType || objType == typeof(string))
            {
                if (memberSerializers.TryGetValue(objType, out var serializer))
                    return serializer(obj) + Environment.NewLine;
                return obj + Environment.NewLine;
            }

            if (!serializedObjects.Add(obj))
                return "Cycle, object was already serialized." + Environment.NewLine;

            return obj switch
            {
                IDictionary dictionary =>
                    SerializerHandler.SerializeDictionary(dictionary, nestingLevel, PrintToString),
                IEnumerable enumerable =>
                    SerializerHandler.SerializeIEnumerable(enumerable, nestingLevel, PrintToString),
                _ => SerializerHandler.SerializeObject(obj, nestingLevel, SerializeMember, IsMemberExcluded)
            };
        }

        private bool IsMemberExcluded(MemberInfo memberInfo)
        {
            return excludedMembers.Contains(memberInfo) ||
                   excludedTypes.Contains(memberInfo.GetMemberType());
        }

        private bool TryGetCustomSerializer(MemberInfo memberInfo, out Func<object, string> serializer)
        {
            return memberSerializers.TryGetValue(memberInfo, out serializer) ||
                   memberSerializers.TryGetValue(memberInfo.GetMemberType(), out serializer);
        }

        private string SerializeMember(object obj, MemberInfo memberInfo, int nestingLevel)
        {
            return TryGetCustomSerializer(memberInfo, out var serializer)
                ? serializer.Invoke(memberInfo.GetValue(obj)) + Environment.NewLine
                : PrintToString(memberInfo.GetValue(obj), nestingLevel);
        }
    }
}
