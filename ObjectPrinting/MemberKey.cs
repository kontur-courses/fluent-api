using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
