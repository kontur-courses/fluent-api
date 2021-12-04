using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Dictionary<Type, Func<object, string>> typeSerializers;
        private readonly Dictionary<MemberInfo, Func<object, string>> memberSerializers;
        
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
            typeSerializers = new Dictionary<Type, Func<object, string>>();
            memberSerializers = new Dictionary<MemberInfo, Func<object, string>>();
        }

        public string PrintToString(object obj, int nestingLevel = 0, bool isPrintObjName = true)
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
            foreach (var memberInfo in GetSerializedMembers(objType, mi => !IsExcluded(mi)))
            {
                var memberIndent = new string('\t', nestingLevel + 1);
                sb.Append($"{memberIndent}{memberInfo.Name} = {SerializeMember(obj, memberInfo, nestingLevel)}");
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

        private string SerializeMember(object obj, MemberInfo memberInfo, int nestingLevel)
        {
            if (TryGetCustomSerializer(memberInfo, out var serializer))
                return serializer.Invoke(memberInfo.GetValue(obj)) + Environment.NewLine;
            
            return PrintToString(memberInfo.GetValue(obj), nestingLevel + 1, false);
        }
        
        private bool TryGetCustomSerializer(MemberInfo memberInfo, out Func<object, string> serializer)
        {
            if (memberSerializers.TryGetValue(memberInfo, out var memberSerializer))
            {
                serializer = memberSerializer;
                return true;
            }

            if (typeSerializers.TryGetValue(memberInfo.GetMemberType(), out var typeSerializer))
            {
                serializer = typeSerializer;
                return true;
            }

            serializer = null;
            return false;
        }
        
        private static IEnumerable<MemberInfo> GetSerializedMembers(Type type, Predicate<MemberInfo> selector)
        {
            return type.GetMembers()
                .Where(mi => mi.MemberType == MemberTypes.Field || mi.MemberType == MemberTypes.Property)
                .Where(mi => selector(mi));
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

        public TypeSerializingConfig<TOwner, TPropType> Serialize<TPropType>()
        {
            return new TypeSerializingConfig<TOwner, TPropType>(this, typeSerializers);
        }

        public PropSerializingConfig<TOwner, TPropType> Serialize<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var body = (MemberExpression) memberSelector.Body;
            return new PropSerializingConfig<TOwner, TPropType>(this, memberSerializers, body.Member);
        }
    }
}