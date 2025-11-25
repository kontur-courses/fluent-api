using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ObjectPrinting;

public record PropertyPath
{
    private List<string> pathSegments;
    public PropertyPath(List<string> pathSegments) => this.pathSegments = pathSegments;
    
    public override string ToString() => string.Join(".", pathSegments);

    public static PropertyPath ConvertFromSelector<TOwner, TPropType>(Expression<Func<TOwner, TPropType>> selector)
    {
        var segments = new List<string>();
        var expression = selector.Body;

        while (expression is MemberExpression member)
        {
            segments.Add(member.Member.Name);
            expression = member.Expression;
        }
        segments.Add(typeof(TOwner).Name);
        
        segments.Reverse();
        return new PropertyPath(segments);
    }
}