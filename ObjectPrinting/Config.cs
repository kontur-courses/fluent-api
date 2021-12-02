using System;
using System.Collections.Generic;
using System.Reflection;
using ObjectPrinting.Extensions;

namespace ObjectPrinting
{
    public class Config : ICloneable
    {
        private HashSet<Type> excludedTypes;
        private HashSet<MemberInfo> excludedMembers;
        private Dictionary<Type, Func<object, string>> altTypeSerializers;
        private Dictionary<MemberInfo, Func<object, string>> altMemberSerializers;

        public Config()
        {
            excludedMembers = new HashSet<MemberInfo>();
            excludedTypes = new HashSet<Type>();
            altMemberSerializers = new Dictionary<MemberInfo, Func<object, string>>();
            altTypeSerializers = new Dictionary<Type, Func<object, string>>();
        }

        public Config(Config config)
        {
            excludedMembers = config.excludedMembers;
            excludedTypes = config.excludedTypes;
            altMemberSerializers = config.altMemberSerializers;
            altTypeSerializers = config.altTypeSerializers;
        }

        public object Clone()
        {
            return new Config()
            {
                excludedMembers = new HashSet<MemberInfo>(excludedMembers),
                excludedTypes = new HashSet<Type>(excludedTypes),
                altMemberSerializers = new Dictionary<MemberInfo, Func<object, string>>(altMemberSerializers),
                altTypeSerializers = new Dictionary<Type, Func<object, string>>(altTypeSerializers),
            };
        }

        protected void AddExcludedType(Type type)
        {
            excludedTypes.Add(type);
        }

        protected void AddExcludedMember(MemberInfo memberInfo)
        {
            excludedMembers.Add(memberInfo);
        }

        protected void AddTypeSerialzier(Type type, Func<object, string> customPrint)
        {
            altTypeSerializers[type] = customPrint;
        }

        protected void AddMemberSerializer(MemberInfo member, Func<object, string> customPrint)
        {
            altMemberSerializers[member] = customPrint;
        }

        protected bool IsExcluded(MemberInfo member)
        {
            return excludedTypes.Contains(member.GetMemberType()) || excludedMembers.Contains(member);
        }

        protected Func<object, string> TryGetAltSerializer(MemberInfo memberInfo)
        {
            if (altMemberSerializers.TryGetValue(memberInfo, out var memberSerializer))
                return memberSerializer;

            return altTypeSerializers.TryGetValue(memberInfo.GetMemberType(), out var typeSerializer)
                ? typeSerializer
                : null;
        }
    }
}