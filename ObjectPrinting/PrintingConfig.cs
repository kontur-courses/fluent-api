using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ObjectPrinting.Extensions;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<(MemberInfo, int)> excludedMembers = new HashSet<(MemberInfo, int)>();
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<object> serializedObjects = new HashSet<object>();

        private readonly Dictionary<(MemberInfo, int), Func<object, string>> memberSerializers =
            new Dictionary<(MemberInfo, int), Func<object, string>>();

        public string PrintToString(TOwner obj) => PrintToString(obj, 0);

        public PrintingConfig<TOwner> ExcludeProperty<TProperty>()
        {
            excludedTypes.Add(typeof(TProperty));
            return this;
        }

        public PrintingConfig<TOwner> ExcludeProperty<TProperty>(Expression<Func<TOwner, TProperty>> memberSelector)
        {
            var body = (MemberExpression)memberSelector.Body;
            var nestingLevel = body.Expression.ToString().Count(x => x == '.') + 1;
            excludedMembers.Add((body.Member, nestingLevel));
            return this;
        }

        public PrintingPropertyConfig<TOwner, TProperty> ChangeSerializationFor<TProperty>() =>
            new PrintingPropertyConfig<TOwner, TProperty>(this, memberSerializers, typeof(TProperty));

        public PrintingPropertyConfig<TOwner, TPropType> ChangeSerializationFor<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var body = (MemberExpression)memberSelector.Body;
            var nestingLevel = body.Expression.ToString().Count(x => x == '.') + 1;
            return new PrintingPropertyConfig<TOwner, TPropType>(this, memberSerializers, body.Member, nestingLevel);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            var objType = obj.GetType();
            if (objType.IsPrimitive || objType.IsValueType || objType == typeof(string))
            {
                if (memberSerializers.TryGetValue((objType, nestingLevel), out var serializer) ||
                    memberSerializers.TryGetValue((objType, -1), out serializer) )
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

        private bool IsMemberExcluded(MemberInfo memberInfo, int nestingLevel)
        {
            return excludedMembers.Contains((memberInfo, nestingLevel)) ||
                   excludedTypes.Contains(memberInfo.GetMemberType());
        }

        private bool TryGetCustomSerializer(MemberInfo memberInfo, int nestingLevel, out Func<object, string> 
                serializer)
        {
            return memberSerializers.TryGetValue((memberInfo, nestingLevel), out serializer) ||
                   memberSerializers.TryGetValue((memberInfo.GetMemberType(), nestingLevel), out serializer)||
                       memberSerializers.TryGetValue((memberInfo, -1), out serializer);
        }

        private string SerializeMember(object obj, MemberInfo memberInfo, int nestingLevel)
        {
            return TryGetCustomSerializer(memberInfo, nestingLevel, out var serializer)
                ? serializer.Invoke(memberInfo.GetValue(obj)) + Environment.NewLine
                : PrintToString(memberInfo.GetValue(obj), nestingLevel);
        }
    }
}
