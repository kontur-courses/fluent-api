using System;
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
        private readonly Dictionary<Type, Delegate> typeSerializers;
        private readonly Dictionary<MemberInfo, Delegate> memberSerializers;
        private readonly Type[] finalTypes;

        public PrintingConfig()
        {
            excludingTypes = new HashSet<Type>();
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

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludingTypes.Add(typeof(TPropType));
            return this;
        }

        public MemberPrintingConfig<TOwner, TPropType> Printing<TPropType>()
            => new MemberPrintingConfig<TOwner, TPropType>(this, null);

        public MemberPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector
        )
            => new MemberPrintingConfig<TOwner, TPropType>(
                this,
                (memberSelector.Body as MemberExpression)?.Member
            );

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public void AddTypeSerializer(Type type, Delegate typeSerializeFunc)
        {
            typeSerializers[type] = typeSerializeFunc;
        }

        public void AddMemberSerializer(MemberInfo memberInfo, Delegate memberSerializeFunc)
        {
            memberSerializers[memberInfo] = memberSerializeFunc;
        }

        private string SerializeMember(object obj, int nestingLevel)
        {
            var sb = new StringBuilder();
            var indentation = new string('\t', nestingLevel + 1);

            var type = obj.GetType();
            sb.AppendLine(type.Name);
            var members = type.GetProperties().Concat<MemberInfo>(type.GetFields());
            foreach (var memberInfo in members.Where(x => !excludingTypes.Contains(x.GetMemberType())))
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

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj is null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            if (typeSerializers.ContainsKey(type))
                obj = typeSerializers[type].DynamicInvoke(obj);
            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;

            return type.Name
                   + Environment.NewLine
                   + SerializeMember(obj, nestingLevel);
        }
    }
}