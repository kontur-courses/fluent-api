using System;
using System.Reflection;


namespace ObjectPrinting
{
    internal record struct MemberKey
    {
        Type DeclaringType;
        string Name;

        public MemberKey(Type declaringType, string name)
        {
            DeclaringType = declaringType;
            Name = name;
        }

        public static MemberKey From(MemberInfo member)
            => new MemberKey(member.DeclaringType!, member.Name);
    }
}
