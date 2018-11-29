using System;
using System.Reflection;

namespace ObjectPrinting
{
    public static class MemberInfoExtensions
    {
        public static Type GetMemberType(this MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo) memberInfo).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo) memberInfo).PropertyType;
                default:
                    throw new NotImplementedException();
            }
        }

        public static object GetValue(this MemberInfo memberInfo, object forObject)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo) memberInfo).GetValue(forObject);
                case MemberTypes.Property:
                    return ((PropertyInfo) memberInfo).GetValue(forObject);
                default:
                    throw new NotImplementedException();
            }
        }

        private static bool CanParticipateInSerialization(this MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                case MemberTypes.Property:
                    return true;
                default:
                    return false;
            }
        }

        public static void CheckCanParticipateInSerialization(this MemberInfo memberInfo)
        {
            if (!memberInfo.CanParticipateInSerialization())
            {
                throw new ArgumentException(
                    $"{memberInfo.MemberType} {memberInfo.Name} cannot participate in serialization!");
            }
        }
    }
}
