using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectPrinting.Solved
{
    public static class TypeExtensions
    {
        //Знаю, что комментарий оставлять плохо, но если стереть вызов ToList(),
        //то фильтрация не работает
        public static IEnumerable<MemberInfo> GetPropertiesAndFields(this Type type)
        {
            return type.GetMembers().Where(member => member.IsFieldOrProperty()).ToList();
        }
    }
}