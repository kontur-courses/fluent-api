using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public class SerializerSettings
    {
        private readonly List<Type> excludedTypes = new();
        private readonly Dictionary<Type, Func<object, string>> typeToSerializer = new();
        private readonly Dictionary<MemberInfo, Func<object, string>> memberSerializer = new();
        private readonly HashSet<MemberInfo> excludedMembers = new();
        public bool AllowCyclingReference { get; set; }

        public bool IsExcluded(Type type) => excludedTypes.Contains(type);

        public bool IsExcluded(MemberInfo member) => excludedMembers.Contains(member);
        
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
            typeToSerializer[typeof(TType)] = obj => serializer((TType) obj);
        }

        public void AddMemberSerializer<TType>(MemberInfo memberInfo, Func<TType, string> serializer)
        {
            memberSerializer[memberInfo] = obj => serializer((TType) obj);
        }

        public bool TryGetMemberSerializer(MemberInfo memberInfo, out Func<object, string> serializer) => 
            memberSerializer.TryGetValue(memberInfo, out serializer);

        public bool TryGetTypeSerializer(Type type, out Func<object, string> serializer) => 
            typeToSerializer.TryGetValue(type, out serializer);
    }
}