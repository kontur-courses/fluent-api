using System.Reflection;

namespace ObjectPrinting
{
    public static class MemberInfoExtensions
    {
        public static bool TryGetValue(this MemberInfo memberInfo, object owner, out object value)
        {
            switch (memberInfo)
            {
                case PropertyInfo propertyInfo:
                    value = propertyInfo.GetValue(owner);
                    return true;
                case FieldInfo fieldInfo:
                    value = fieldInfo.GetValue(owner);
                    return true;
            }

            value = null;
            return false;
        }
    }
}
