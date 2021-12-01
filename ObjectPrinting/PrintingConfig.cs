using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludingTypes;
        private readonly HashSet<MemberInfo> excludingMembers;
        private readonly HashSet<object> serializedObjects;
        private readonly Dictionary<Type, Delegate> typeSerializers;
        private readonly Dictionary<MemberInfo, Delegate> memberSerializers;
        private readonly Type[] finalTypes;

        public PrintingConfig()
        {
            excludingTypes = new HashSet<Type>();
            excludingMembers = new HashSet<MemberInfo>();
            serializedObjects = new HashSet<object>();
            typeSerializers = new Dictionary<Type, Delegate>();
            memberSerializers = new Dictionary<MemberInfo, Delegate>();
            finalTypes = new[]
            {
                typeof(int),
                typeof(double),
                typeof(float),
                typeof(string),
                typeof(DateTime),
                typeof(TimeSpan),
                typeof(Guid)
            };
        }

        public PrintingConfig<TOwner> Excluding<TMemType>()
        {
            excludingTypes.Add(typeof(TMemType));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TMemType>(
            Expression<Func<TOwner, TMemType>> memberSelector
        )
        {
            excludingMembers.Add((memberSelector.Body as MemberExpression)?.Member);
            return this;
        }

        public MemberPrintingConfig<TOwner, TMemType> Printing<TMemType>()
            => new MemberPrintingConfig<TOwner, TMemType>(this, null);

        public MemberPrintingConfig<TOwner, TMemType> Printing<TMemType>(
            Expression<Func<TOwner, TMemType>> memberSelector
        )
            => new MemberPrintingConfig<TOwner, TMemType>(
                this,
                (memberSelector.Body as MemberExpression)?.Member
            );

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        internal void AddTypeSerializer(Type type, Delegate typeSerializeFunc)
        {
            typeSerializers[type] = typeSerializeFunc;
        }

        internal void AddMemberSerializer(MemberInfo memberInfo, Delegate memberSerializeFunc)
        {
            memberSerializers[memberInfo] = memberSerializeFunc;
        }

        private string SerializeMember(object obj, int nestingLevel)
        {
            if (serializedObjects.Contains(obj))
                return "cyclical link" + Environment.NewLine;
            serializedObjects.Add(obj);
            
            var sb = new StringBuilder();
            var indentation = new string('\t', nestingLevel + 1);

            var type = obj.GetType();
            sb.AppendLine(type.Name);
            var members = type.GetProperties().Concat<MemberInfo>(type.GetFields());
            foreach (
                var memberInfo in members
                    .Where(x => !excludingTypes.Contains(x.GetMemberType()))
                    .Where(x => !excludingMembers.Contains(x))
            )
            {
                var serializedObject = TryGetSerializer(memberInfo, out var serializer)
                    ? serializer.DynamicInvoke(memberInfo.GetValue(obj)) + Environment.NewLine
                    : PrintToString(memberInfo.GetValue(obj), nestingLevel + 1);
                sb.Append(indentation + memberInfo.Name + " = " + serializedObject);
            }

            return sb.ToString();
        }

        private bool TryGetSerializer(MemberInfo memberInfo, out Delegate serializer)
        {
            if (memberSerializers.TryGetValue(memberInfo, out serializer))
                return true;
            var type = memberInfo.GetType();
            return typeSerializers.TryGetValue(type, out serializer);
        }

        private string SerializeEnumerable(IEnumerable enumerable, int nestingLevel)
        {
            var serializedEnumerable = enumerable
                .Cast<object>()
                .Select(x => PrintToString(x, nestingLevel + 1));
            return "(" + string.Join("\t", serializedEnumerable) + ")";
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj is null)
                return "null" + Environment.NewLine;
            var type = obj.GetType();
            if (typeSerializers.ContainsKey(type))
                obj = typeSerializers[type].DynamicInvoke(obj);
            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;
            if (obj is IEnumerable enumerable)
                return SerializeEnumerable(enumerable, nestingLevel);
            
            return type.Name
                   + Environment.NewLine
                   + SerializeMember(obj, nestingLevel);
        }
    }
}