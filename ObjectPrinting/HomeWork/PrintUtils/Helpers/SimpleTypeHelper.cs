namespace ObjectPrinting.HomeWork.PrintUtils.Helpers;

public static class SimpleTypeHelper
{
    public static bool IsSimple(Type type)
    {
        return type.IsPrimitive
               || type == typeof(string)
               || type == typeof(decimal)
               || type == typeof(DateTime)
               || type == typeof(Guid)
               || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                   IsSimple(type.GetGenericArguments()[0]));
    }
}