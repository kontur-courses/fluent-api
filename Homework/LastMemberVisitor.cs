using System.Linq.Expressions;

namespace Homework;

public class LastMemberVisitor : ExpressionVisitor
{
    public MemberExpression? LastMemberExpression { get; private set; }

    protected override Expression VisitMember(MemberExpression node)
    {
        LastMemberExpression = node;

        return base.VisitMember(node);
    }
}
