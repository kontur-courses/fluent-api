using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public class SerializerSettings
    {
        private readonly List<Type> excludedTypes = new();
        private readonly Dictionary<Type, Delegate> typeToSerializer = new();
        private readonly Dictionary<MemberInfo, Delegate> memberSerializer = new();
        private readonly HashSet<MemberInfo> excludedMembers = new();
        public bool IsAllowCyclingReference { get; set; }

        public bool IsExcluded(Type type) => excludedTypes.Contains(type);

        public bool IsExcluded(MemberInfo member) => excludedMembers.Contains(member);

        public bool HasMemberSerializer(MemberInfo memberInfo) => memberSerializer.ContainsKey(memberInfo);
        public bool HasTypeSerializer(Type type) => typeToSerializer.ContainsKey(type);

        public Delegate GetMemberSerializer(MemberInfo memberInfo) => memberSerializer[memberInfo];
        public Delegate GetTypeSerializer(Type type) => typeToSerializer[type];

        public void AddExcludedType(Type type)
        {
            excludedTypes.Add(type);
        }

        public void AddExcludedMember(MemberInfo memberInfo)
        {
            excludedMembers.Add(memberInfo);
        }

        public void AddTypeSerializer<TType>(Func<TType, string> serializer)
        {
            typeToSerializer[typeof(TType)] = serializer;
        }

        public void AddMemberSerializer<TType>(MemberInfo memberInfo, Func<TType, string> serializer)
        {
            memberSerializer[memberInfo] = serializer;
        }
    }
}