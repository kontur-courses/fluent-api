using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public class Config
    {
        public readonly HashSet<Type> ExcludedTypes;
        public readonly HashSet<MemberInfo> ExcludedMembers;
        public readonly Dictionary<Type, Func<object, string>> AltTypeSerializers;
        public readonly Dictionary<MemberInfo, Func<object, string>> AltMemberSerializers;

        public Config()
        {
            ExcludedMembers = new HashSet<MemberInfo>();
            ExcludedTypes = new HashSet<Type>();
            AltMemberSerializers = new Dictionary<MemberInfo, Func<object, string>>();
            AltTypeSerializers = new Dictionary<Type, Func<object, string>>();
        }
    }
}