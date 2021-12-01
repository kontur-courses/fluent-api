using System;
using System.Linq;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public static class ExpressionExt
    {
        public static string GetPropertyName<T, TType>(this Expression<Func<T, TType>> expression)
        {
            return string.Join(".", expression.ToString().Split('.').Skip(1)).Split(',').First();
        }
    }
}