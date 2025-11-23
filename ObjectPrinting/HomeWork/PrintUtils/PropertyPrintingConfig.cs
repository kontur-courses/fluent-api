using System.Linq.Expressions;
using System.Reflection;
using ObjectPrinting.HomeWork.RuleUtils.Interfaces;
using ObjectPrinting.HomeWork.RuleUtils.Strategies.Implementations;

namespace ObjectPrinting.HomeWork.PrintUtils;

public class PropertyPrintingConfig<TOwner, TProp>(
    IRuleProcessor rules,
    PrintingConfig<TOwner> parent,
    Expression<Func<TOwner, TProp>> selector)
{
    private readonly PropertyInfo property = GetProperty(selector);

    public PropertyPrintingConfig<TOwner, TProp> Serialize(Func<TProp, string> serializer)
    {
        rules.AddRule(new SerializationRule<TProp>(serializer, property));
        return this;
    }

    public PropertyPrintingConfig<TOwner, string> Trim(int length)
    {
        if (typeof(TProp) != typeof(string))
            throw new InvalidOperationException("Trim доступен только для строк.");

        rules.AddRule(new TrimStringRule(property, length));
        return (PropertyPrintingConfig<TOwner, string>)(object)this;
    }

    public PropertyPrintingConfig<TOwner, TProp> Exclude()
    {
        rules.AddRule(new ExcludeRule(property));
        return this;
    }

    public PrintingConfig<TOwner> Apply() => parent;

    private static PropertyInfo GetProperty<T>(Expression<Func<TOwner, T>> exp) =>
        (PropertyInfo)((MemberExpression)exp.Body).Member;
}