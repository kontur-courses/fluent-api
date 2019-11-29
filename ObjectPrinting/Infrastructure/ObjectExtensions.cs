using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ObjectPrinting.Core;

namespace ObjectPrinting.Infrastructure
{
    public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }
        
        public static IEnumerable<ItemInfo> GetItems(this object obj)
        {
            if (obj is IEnumerable collection)
            {
                foreach (var item in collection)
                {
                    yield return new ItemInfo(item, item.GetType());
                }
            }
            else
            {
                var objType = obj.GetType();
                foreach (var propertyInfo in objType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    yield return new ItemInfo(propertyInfo.GetValue(obj),
                        propertyInfo.PropertyType, propertyInfo.Name);
                }
                foreach (var fieldInfo in objType.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    yield return new ItemInfo(fieldInfo.GetValue(obj),
                        fieldInfo.FieldType, fieldInfo.Name);
                }
            }
        }
    }
}