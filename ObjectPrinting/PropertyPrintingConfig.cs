using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TProperty> : PrintingConfig<TOwner>
{
    private readonly MemberInfo memberInfo;

    public PropertyPrintingConfig(Expression<Func<TOwner, TProperty>> expression,
        PrintingConfig<TOwner> parentConfig) : base(parentConfig)
    {
        if (parentConfig == null) throw new ArgumentNullException(nameof(parentConfig));
        var body = (MemberExpression)expression.Body;
        memberInfo = body.Member;
        var valid = memberInfo is PropertyInfo || memberInfo is FieldInfo;
        if (!valid)
            throw new ArgumentException("invalid expression", nameof(body));
    }

    public PropertyPrintingConfig<TOwner, TProperty> Serialize(Func<TProperty, string> func)
    {
        ((IInternalPrintingConfig<TOwner>)this).GetRoot().MemberSerializers[memberInfo] = d => func((TProperty)d);
        return this;
    }

    public PropertyPrintingConfig<TOwner, TProperty> Exclude()
    {
        ((IInternalPrintingConfig<TOwner>)this).GetRoot().MemberExcluding.Add(memberInfo);
        return this;
    }
}