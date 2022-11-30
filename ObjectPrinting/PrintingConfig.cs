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
        private readonly HashSet<MemberInfo> excludedMembers;
        private readonly HashSet<Type> excludedTypes;
        private readonly HashSet<Type> finalTypes;
        private readonly Dictionary<MemberInfo, Func<object, string>> memberSerializers;
        private readonly HashSet<object> serializedObjects;

        public PrintingConfig()
        {
            serializedObjects = new HashSet<object>();
            finalTypes = new HashSet<Type>
            {
                typeof(int), typeof(double), typeof(float),
                typeof(bool), typeof(long), typeof(Guid),
                typeof(string), typeof(DateTime), typeof(TimeSpan)
            };
            excludedTypes = new HashSet<Type>();
            excludedMembers = new HashSet<MemberInfo>();
            memberSerializers = new Dictionary<MemberInfo, Func<object, string>>();
        }

        public string PrintToString(object obj, int depth = 0)
        {
            if (obj == null)
                return "null";

            if (finalTypes.Contains(obj.GetType()))
                return obj.ToString();

            if (serializedObjects.Contains(obj))
                return "Обнаружен цикл! Объект будет пропущен." + Environment.NewLine;
            serializedObjects.Add(obj);

            if (obj is IDictionary dictionary)
                return SerializeDictionary(dictionary, depth);
            if (obj is IEnumerable collection)
                return SerializeCollection(collection, depth);
            return SerializeObject(obj, depth);
        }

        private string SerializeCollection(IEnumerable collection, int depth)
        {
            var sb = new StringBuilder(collection.GetType().Name + " {" + Environment.NewLine);
            foreach (var item in collection)
                sb.Append(PrintToString(item, depth + 1));
            sb.Append(new string('\t', depth) + '}');
            return sb.ToString();
        }

        private string SerializeDictionary(IDictionary dictionary, int depth)
        {
            var sb = new StringBuilder(dictionary.GetType().Name + " {" + Environment.NewLine);
            foreach (var key in dictionary.Keys)
            {
                sb.Append(new string('\t', depth + 1));
                sb.Append(PrintToString(key, depth + 1).Trim() + " = ");
                sb.Append(PrintToString(dictionary[key]).Trim());
                sb.Append(Environment.NewLine);
            }
            sb.Append(new string('\t', depth) + '}');

            return sb.ToString();
        }

        private string SerializeObject(object obj, int depth)
        {
            var objType = obj.GetType();
            var indent = new string('\t', depth);

            var name = indent + obj.GetType().Name + Environment.NewLine;

            var sb = new StringBuilder(name);
            foreach (var memberInfo in GetSerializedMembers(objType, t => !IsExcluded(t)))
            {
                var memberIndent = new string('\t', depth + 1);
                sb.Append($"{memberIndent}{memberInfo.Name} = {SerializeMember(obj, memberInfo, depth)}" +
                          Environment.NewLine);
            }

            return sb.ToString();
        }

        private static IEnumerable<MemberInfo> GetSerializedMembers(Type type, Predicate<MemberInfo> selector)
        {
            return type.GetMembers()
                .Where(t => t.MemberType == MemberTypes.Field || t.MemberType == MemberTypes.Property)
                .Where(t => selector(t));
        }

        private bool IsExcluded(MemberInfo memberInfo)
        {
            return excludedMembers.Contains(memberInfo) ||
                   excludedTypes.Contains(memberInfo.GetMemberType());
        }

        private string SerializeMember(object obj, MemberInfo memberInfo, int depth)
        {
            if (TryGetCustomSerializer(memberInfo, out var serializer))
                return serializer.Invoke(memberInfo.GetValue(obj));

            return PrintToString(memberInfo.GetValue(obj), depth + 1);
        }

        private bool TryGetCustomSerializer(MemberInfo memberInfo, out Func<object, string> serializer)
        {
            if (memberSerializers.TryGetValue(memberInfo, out serializer)) return true;

            return memberSerializers.TryGetValue(memberInfo.GetMemberType(), out serializer);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberSerializers, typeof(TPropType));
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var body = (MemberExpression)memberSelector.Body;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberSerializers, body.Member);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var body = (MemberExpression)memberSelector.Body;
            excludedMembers.Add(body.Member);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }
    }
}