using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ObjectPrinting.Extensions;

namespace ObjectPrinting
{
    public class SerializationSettings
    {
        private readonly Dictionary<MemberInfo, Func<object, string>> customMembersSerializers = new();
        private readonly Dictionary<Type, Func<object, string>> customTypesSerializers = new();

        private readonly HashSet<MemberInfo> excludedMembers = new();
        private readonly HashSet<Type> excludedTypes = new();
        
        public bool AreCycleReferencesAllowed { get; set; }

        public bool TryGetSerializer(MemberInfo memberInfo, out Func<object, string> serializer) =>
            customMembersSerializers.TryGetValue(memberInfo, out serializer) 
            || customTypesSerializers.TryGetValue(memberInfo.GetMemberType(), out serializer);

        public void Exclude(Type type) => excludedTypes.Add(type);

        public void Exclude(MemberInfo selectMember) => excludedMembers.Add(selectMember);

        public bool IsExcluded(MemberInfo memberInfo)
            => excludedMembers.Contains(memberInfo) || excludedTypes.Contains(memberInfo.GetMemberType());
        
        public void SetSerializer<TType>(Func<TType, string> transformer)
        {
            if (transformer == null) 
                throw new ArgumentNullException(nameof(transformer));
            customTypesSerializers[typeof(TType)] = obj => transformer((TType)obj);
        }

        public void SetSerializer<TType>(MemberInfo memberInfo, Func<TType, string> transformer)
        {
            if (memberInfo == null) 
                throw new ArgumentNullException(nameof(memberInfo));
            if (transformer == null) 
                throw new ArgumentNullException(nameof(transformer));
            customMembersSerializers[memberInfo] = obj => transformer((TType)obj);
        }
    }
}