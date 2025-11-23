using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using ObjectPrinting.HomeWork.PrintUtils.Interfaces;
using ObjectPrinting.HomeWork.RuleUtils.Interfaces;
using ObjectPrinting.HomeWork.RuleUtils.Strategies.Implementations;

namespace ObjectPrinting.HomeWork.PrintUtils;

public class PrintingConfig<TOwner>(IRuleProcessor rp, IPrintingProcessor pr)
{
    public string PrintToString(TOwner obj) => pr.Print(obj, 0, []);

    public TypePrintingConfig<TOwner, TType> For<TType>() => new(rp, this);
    public PropertyPrintingConfig<TOwner, TProp> For<TProp>(Expression<Func<TOwner, TProp>> selector)
        => new(rp, this, selector);
    
    public PrintingConfig<TOwner> Exclude<T>()
    {
        rp.AddRule(new ExcludeRule(typeof(T)));
        return this;
    }

    public PrintingConfig<TOwner> Exclude<TProp>(Expression<Func<TOwner, TProp>> selector)
    {
        var prop = GetProperty(selector);
        rp.AddRule(new ExcludeRule(prop));
        return this;
    }

    public PrintingConfig<TOwner> Trim(Expression<Func<TOwner, string>> selector, int length)
    {
        var prop = GetProperty(selector);
        rp.AddRule(new TrimStringRule(prop, length));
        return this;
    }

    public PrintingConfig<TOwner> Serialize<TType>(Func<TType, string> serializer)
    {
        rp.AddRule(new SerializationRule<TType>(serializer));
        return this;
    }

    public PrintingConfig<TOwner> Serialize<TProp>(Expression<Func<TOwner, TProp>> selector, 
        Func<TProp, string> serializer)
    {
        var prop = GetProperty(selector);
        rp.AddRule(new SerializationRule<TProp>(serializer, prop));
        return this;
    }

    public PrintingConfig<TOwner> SetFormattingCulture(CultureInfo culture)
    {
        rp.AddRule(new CultureRule(culture));
        return this;
    }

    private static PropertyInfo GetProperty<T>(Expression<Func<TOwner, T>> expr) 
        => (PropertyInfo)((MemberExpression)expr.Body).Member;
}