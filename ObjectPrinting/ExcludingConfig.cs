using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    internal class ExcludingConfig
    {
        private readonly HashSet<MemberInfo> excludedMembers = new HashSet<MemberInfo>();

        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();

        public bool IsExcluded(MemberInfo memberInfo)
            => excludedMembers.Contains(memberInfo) ||
               excludedTypes.Contains(memberInfo.GetReturnType());
        
        public void Exclude(MemberInfo member)
        {
            excludedMembers.Add(member);
        }

        public void Exclude<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
        }
    }
}