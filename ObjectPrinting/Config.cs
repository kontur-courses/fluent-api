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
        private HashSet<Type> finalTypes;

        public Config()
        {
            excludedMembers = new HashSet<MemberInfo>();
            excludedTypes = new HashSet<Type>();
            altMemberSerializers = new Dictionary<MemberInfo, Func<object, string>>();
            altTypeSerializers = new Dictionary<Type, Func<object, string>>();
            finalTypes = new HashSet<Type>
            {
                typeof(int),
                typeof(uint),
                typeof(double),
                typeof(float),
                typeof(string),
                typeof(char),
                typeof(DateTime),
                typeof(TimeSpan),
                typeof(Guid),
                typeof(long),
                typeof(ulong),
                typeof(decimal),
                typeof(bool),
                typeof(byte),
                typeof(sbyte),
                typeof(short),
                typeof(ushort),
            };
        }

        public Config(Config config)
        {
            excludedMembers = config.excludedMembers;
            excludedTypes = config.excludedTypes;
            altMemberSerializers = config.altMemberSerializers;
            altTypeSerializers = config.altTypeSerializers;
            finalTypes = config.finalTypes;
        }

        public object Clone()
        {
            return new Config()
            {
                excludedMembers = new HashSet<MemberInfo>(excludedMembers),
                excludedTypes = new HashSet<Type>(excludedTypes),
                altMemberSerializers = new Dictionary<MemberInfo, Func<object, string>>(altMemberSerializers),
                altTypeSerializers = new Dictionary<Type, Func<object, string>>(altTypeSerializers),
                finalTypes = new HashSet<Type>(finalTypes)
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

        protected bool IsFinalType(Type type)
        {
            return finalTypes.Contains(type);
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