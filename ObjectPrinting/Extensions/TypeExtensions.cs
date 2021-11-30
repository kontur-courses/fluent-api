using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectPrinting
{
    internal static class TypeExtensions
    {
        public static List<MemberInfo> GetAllMembersOfType<TOwner, TType>(Type[] finalTypes)
        {
            var result = new List<MemberInfo>();
            foreach (var m in typeof(TOwner).GetSerializedMembers())
                AddMembers<TOwner, TType>(m, result, finalTypes);
            return result;
        }

        private static void AddMembers<TOwner, TType>
            (MemberInfo member, List<MemberInfo> members, Type[] finalTypes)
        {
            var type = member.GetTypeOfPropertyOrField();
            if (members.Contains(member) || type == typeof(TOwner))
                return;
            if (type == typeof(TType))
                members.Add(member);
            if (finalTypes.Contains(type))
                return;
            foreach (var m in type.GetSerializedMembers())
                AddMembers<TOwner, TType>(m, members, finalTypes);
        }
    }
}