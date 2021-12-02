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
        private readonly SerializerRepository serializerRepository;
        private readonly Type[] finalTypes;

        public PrintingConfig()
        {
            excludingTypes = new HashSet<Type>();
            excludingMembers = new HashSet<MemberInfo>();
            serializedObjects = new HashSet<object>();
            serializerRepository = new SerializerRepository();
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
            excludingMembers.Add(GetMemberInfo(memberSelector));
            return this;
        }

        public MemberPrintingConfig<TOwner, TMemType> Printing<TMemType>()
            => new MemberPrintingConfig<TOwner, TMemType>(this, null, serializerRepository);

        public MemberPrintingConfig<TOwner, TMemType> Printing<TMemType>(
            Expression<Func<TOwner, TMemType>> memberSelector
        )
            => new MemberPrintingConfig<TOwner, TMemType>(
                this,
                GetMemberInfo(memberSelector),
                serializerRepository
            );

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string SerializeMember(object obj, int nestingLevel)
        {
            if (serializedObjects.Contains(obj))
                return "cyclical reference" + Environment.NewLine;
            serializedObjects.Add(obj);
            
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            var type = obj.GetType();
            sb.AppendLine(type.Name);
            var members = type.GetProperties().Concat<MemberInfo>(type.GetFields());
            foreach (
                var memberInfo in members
                    .Where(x => !excludingTypes.Contains(GetMemberType(x)))
                    .Where(x => !excludingMembers.Contains(x))
            )
            {
                var serializedObject = TryGetSerializer(memberInfo, out var serializer)
                    ? serializer.DynamicInvoke(GetMemberValue(memberInfo, obj)) + Environment.NewLine
                    : PrintToString(GetMemberValue(memberInfo, obj), nestingLevel + 1);
                sb.Append(indentation);
                sb.Append(memberInfo.Name);
                sb.Append(" = ");
                sb.Append(serializedObject);
            }

            return sb.ToString();
        }

        private bool TryGetSerializer(MemberInfo memberInfo, out Delegate serializer)
        {
            if (serializerRepository.MemberSerializers.TryGetValue(memberInfo, out serializer))
                return true;
            var type = memberInfo.GetType();
            return serializerRepository.TypeSerializers.TryGetValue(type, out serializer);
        }

        private string SerializeEnumerable(IEnumerable enumerable, int nestingLevel)
        {
            var sb = new StringBuilder($"{enumerable.GetType().Name}{Environment.NewLine}");
            foreach (var item in enumerable.Cast<object>())
                sb.Append(PrintToString(item, nestingLevel + 1));
            return "(" + sb + ")";
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj is null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();

            if (serializerRepository.TypeSerializers.ContainsKey(type))
                obj = serializerRepository.TypeSerializers[type].DynamicInvoke(obj);
            
            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;
            
            if (obj is IEnumerable enumerable)
                return SerializeEnumerable(enumerable, nestingLevel);

            return SerializeMember(obj, nestingLevel);
        }

        private static Type GetMemberType(MemberInfo memberInfo)
            => memberInfo switch
            {
                PropertyInfo propertyInfo => propertyInfo.PropertyType,
                FieldInfo fieldInfo => fieldInfo.FieldType,
                _ => throw new NotImplementedException()
            };

        private static object GetMemberValue(MemberInfo memberInfo, object obj)
            => memberInfo switch
            {
                PropertyInfo propertyInfo => propertyInfo.GetValue(obj),
                FieldInfo fieldInfo => fieldInfo.GetValue(obj),
                _ => throw new NotImplementedException()
            };

        private static MemberInfo GetMemberInfo<TMemType>(Expression<Func<TOwner, TMemType>> memberSelector)
        {
            return (memberSelector.Body as MemberExpression)?.Member;
        }
    }
}