using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    internal static class MemberSelector
    {
        internal static MemberInfo GetMember<TRoot, TValue>(
            Expression<Func<TRoot, TValue>> memberSelector)
        {
            ArgumentNullException.ThrowIfNull(memberSelector);

            Expression expression = memberSelector.Body;

            while (expression is UnaryExpression unaryExpression &&
                   (unaryExpression.NodeType == ExpressionType.Convert ||
                    unaryExpression.NodeType == ExpressionType.ConvertChecked))
            {
                expression = unaryExpression.Operand;
            }

            if (expression is not MemberExpression memberExpression ||
                memberExpression.Expression != memberSelector.Parameters[0] ||
                memberExpression.Member is not (PropertyInfo or FieldInfo))
            {
                throw new ArgumentException(
                    "Выражение должно выбирать свойство или поле из своего параметра",
                    nameof(memberSelector));
            }

            return memberExpression.Member;
        }
    }
}