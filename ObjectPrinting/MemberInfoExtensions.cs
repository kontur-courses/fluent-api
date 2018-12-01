using System.Reflection;

namespace ObjectPrinting
{
    public static class MemberInfoExtensions
    {
        public static bool TryGetValue(this MemberInfo memberInfo, object obj, out object value)
        {
            value = null;
            switch (memberInfo)
            {
                case PropertyInfo propertyInfo:
                    value = propertyInfo.GetValue(obj);
                    return true;
                case FieldInfo fieldInfo:
                    value = fieldInfo.GetValue(obj);
                    return true;
                default:
                    return false;
            }
        }
    }
}
