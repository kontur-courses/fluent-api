using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyMapper<T>
    {
        public virtual PropertyInfo PropertyInfo<U>(Expression<Func<T, U>> expression)
        {
            if (expression.Body is MemberExpression member && member.Member is PropertyInfo)
                return (PropertyInfo) member.Member;

            throw new ArgumentException("Expression is not a Property", nameof(expression));
        }

        public virtual string PropertyName<U>(Expression<Func<T, U>> expression)
        {
            return PropertyInfo<U>(expression).Name;
        }

        public virtual Type PropertyType<U>(Expression<Func<T, U>> expression)
        {
            return PropertyInfo<U>(expression).PropertyType;
        }
    }
}