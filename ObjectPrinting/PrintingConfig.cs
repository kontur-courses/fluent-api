using System;
using System.Collections;
using System.Collections.Generic;
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
        private readonly SerializerRepository serializerRepository;
        private readonly Type[] finalTypes;

        public PrintingConfig()
        {
            excludingTypes = new HashSet<Type>();
            excludingMembers = new HashSet<MemberInfo>();
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
            return PrintToString(obj, Environment.NewLine);
        }

        private string PrintToString(TOwner obj, string delimiter)
        {
            if (obj is null)
                return "null" + delimiter;

            return PrintToString(obj, 0, delimiter).ToString();
        }

        private StringBuilder PrintToString(object obj, int nestingLevel, string delimiter)
        {
            var sb = new StringBuilder();
            if (obj is null)
                return sb.Append("null", delimiter);

            var type = obj.GetType();

            if (finalTypes.Contains(type))
                return sb.Append(obj.ToString(), delimiter);

            if (obj is IEnumerable enumerable)
                return sb.Append("(", SerializeEnumerable(enumerable, nestingLevel, delimiter).ToString(), ")");

            return SerializeMember(obj, nestingLevel, delimiter);
        }

        private StringBuilder SerializeMember(object obj, int nestingLevel, string delimiter)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            var type = obj.GetType();
            sb.Append(type.Name, delimiter);
            var members = type.GetProperties().Concat<MemberInfo>(type.GetFields());
            foreach (
                var memberInfo in members
                    .Where(x => !excludingTypes.Contains(GetMemberType(x)))
                    .Where(x => !excludingMembers.Contains(x))
            )
            {
                sb.Append(indentation, memberInfo.Name, " = ");
                if (TryGetSerializer(memberInfo, out var serializer))
                    sb.Append((string) serializer.DynamicInvoke(GetMemberValue(memberInfo, obj)), delimiter);
                else
                    sb.Append(PrintToString(GetMemberValue(memberInfo, obj), nestingLevel + 1, delimiter));
            }

            return sb;
        }

        private bool TryGetSerializer(MemberInfo memberInfo, out Delegate serializer)
        {
            if (serializerRepository.MemberSerializers.TryGetValue(memberInfo, out serializer))
                return true;
            var type = GetMemberType(memberInfo);
            return serializerRepository.TypeSerializers.TryGetValue(type, out serializer);
        }

        private StringBuilder SerializeEnumerable(IEnumerable enumerable, int nestingLevel, string delimiter)
        {
            var sb = new StringBuilder($"{enumerable.GetType().Name}{delimiter}");
            foreach (var item in enumerable.Cast<object>())
                sb.Append(PrintToString(item, nestingLevel + 1, delimiter));
            return sb;
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