using System;
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
        private readonly HashSet<Type> finalTypes;
        private readonly HashSet<Type> excludedTypes;
        private readonly HashSet<MemberInfo> excludedMembers;
        private readonly Dictionary<Type, Delegate> typeSerializers;
        private readonly Dictionary<MemberInfo, Delegate> memberSerializers;

        public PrintingConfig()
        {
            finalTypes = new HashSet<Type>()
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

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var objType = obj.GetType();
            if (finalTypes.Contains(objType))
                return obj + Environment.NewLine;

            var sb = new StringBuilder(objType.Name + Environment.NewLine);
            foreach (var memberInfo in objType.GetMembers()
                .Where(mi => mi.MemberType == MemberTypes.Field || mi.MemberType == MemberTypes.Property)
                .Where(mi => !IsExcluded(mi)))
                sb.Append(SerializeMember(obj, memberInfo, nestingLevel));
            return sb.ToString();
        }

        private bool IsExcluded(MemberInfo memberInfo)
        {
            return excludedMembers.Contains(memberInfo) || 
                   excludedTypes.Contains(memberInfo.GetMemberType());
        }
        
        private string SerializeMember(object obj, MemberInfo memberInfo, int nestingLevel)
        {
            var result = new string('\t', nestingLevel + 1);

            if (memberSerializers.TryGetValue(memberInfo, out var memberSerializer))
                result += memberSerializer.DynamicInvoke(memberInfo.GetValue(obj)) + Environment.NewLine;
            else if (typeSerializers.TryGetValue(memberInfo.GetMemberType(), out var typeSerializer))
                result += memberInfo.Name + " = " + typeSerializer.DynamicInvoke(memberInfo.GetValue(obj)) + Environment.NewLine;
            else
                result += memberInfo.Name + " = " + PrintToString(memberInfo.GetValue(obj), nestingLevel + 1);

            return result;
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