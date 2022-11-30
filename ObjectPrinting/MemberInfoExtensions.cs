using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public static class MemberInfoExtensions
    {
        public static object GetValue(this MemberInfo member, object srcObject)
        {
            switch (member)
            {
                case FieldInfo mfi:
                    return mfi.GetValue(srcObject);
                case PropertyInfo mpi:
                    return mpi.GetValue(srcObject);
                default:
                    return null!;
            }
        }

        public static Type GetMemberType(this MemberInfo member)
        {
            switch (member)
            {
                case FieldInfo mfi:
                    return mfi.FieldType;
                case PropertyInfo mpi:
                    return mpi.PropertyType;
                default:
                    return null!;
            }
        }
    }
}
