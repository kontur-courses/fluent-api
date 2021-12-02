using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Common.Configs
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<object> serializedObjs;
        private readonly HashSet<Type> finalTypes;
        private readonly HashSet<Type> excludedTypes;
        private readonly HashSet<MemberInfo> excludedMembers;
        private readonly Dictionary<Type, Delegate> typeSerializers;
        private readonly Dictionary<MemberInfo, Delegate> memberSerializers;

        public PrintingConfig()
        {
            serializedObjs = new HashSet<object>();
            finalTypes = new HashSet<Type>
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(Guid), typeof(DateTime), typeof(TimeSpan)
            };
            excludedTypes = new HashSet<Type>();
            excludedMembers = new HashSet<MemberInfo>();
            typeSerializers = new Dictionary<Type, Delegate>();
            memberSerializers = new Dictionary<MemberInfo, Delegate>();
        }


        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel, bool isPrintObjName = true)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            if (serializedObjs.Contains(obj))
                return "Reference cycle detected! This object will be skipped";
            serializedObjs.Add(obj);

            return obj is IEnumerable collection
                ? SerializeCollection(collection, nestingLevel)
                : SerializeObject(obj, nestingLevel, isPrintObjName);
        }

        private bool IsExcluded(MemberInfo memberInfo)
        {
            return excludedMembers.Contains(memberInfo) ||
                   excludedTypes.Contains(memberInfo.GetMemberType());
        }

        private string SerializeObject(object obj, int nestingLevel, bool isPrintObjName)
        {
            var objType = obj.GetType();
            var indent = new string('\t', nestingLevel);

            var objName = isPrintObjName
                ? indent + objType.Name + Environment.NewLine
                : Environment.NewLine;

            var sb = new StringBuilder(objName);
            foreach (var memberInfo in objType.GetSerializedMembers(mi => !IsExcluded(mi)))
            {
                var memberIndent = new string('\t', nestingLevel + 1);
                var result = memberIndent + memberInfo.Name + " = ";
                if (memberSerializers.TryGetValue(memberInfo, out var memberSerializer))
                    result += memberSerializer.DynamicInvoke(memberInfo.GetValue(obj)) + Environment.NewLine;
                else if (typeSerializers.TryGetValue(memberInfo.GetMemberType(), out var typeSerializer))
                    result += typeSerializer.DynamicInvoke(memberInfo.GetValue(obj)) + Environment.NewLine;
                else
                    result += PrintToString(memberInfo.GetValue(obj), nestingLevel + 1, false);
                sb.Append(result);
            }

            return sb.ToString();
        }

        private string SerializeCollection(IEnumerable collection, int nestingLevel)
        {
            if (collection is IDictionary dictionary)
                return SerializeDictionary(dictionary, nestingLevel);

            var sb = new StringBuilder(collection.GetType().Name + Environment.NewLine);
            foreach (var item in collection)
                sb.Append(PrintToString(item, nestingLevel + 1));

            return sb.ToString();
        }

        private string SerializeDictionary(IDictionary dictionary, int nestingLevel)
        {
            var sb = new StringBuilder(dictionary.GetType().Name + Environment.NewLine);
            foreach (var key in dictionary.Keys)
            {
                sb.Append(new string('\t', nestingLevel + 1));
                sb.Append(PrintToString(key, nestingLevel + 1).Trim() + " - ");
                sb.Append(PrintToString(dictionary[key], 0).Trim());
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }


        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var body = (MemberExpression) memberSelector.Body;
            excludedMembers.Add(body.Member);
            return this;
        }

        public SerializingConfig<TOwner, TPropType> Serialize<TPropType>()
        {
            return new SerializingConfig<TOwner, TPropType>(this, typeSerializers);
        }

        public SerializingConfig<TOwner, TPropType> Serialize<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var body = (MemberExpression) memberSelector.Body;
            return new SerializingConfig<TOwner, TPropType>(this, memberSerializers, body.Member);
        }
    }
}