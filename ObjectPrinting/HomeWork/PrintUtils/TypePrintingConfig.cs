using System.Globalization;
using ObjectPrinting.HomeWork.RuleUtils.Interfaces;
using ObjectPrinting.HomeWork.RuleUtils.Strategies.Implementations;

namespace ObjectPrinting.HomeWork.PrintUtils;

public class TypePrintingConfig<TOwner, TType>(IRuleProcessor rules, PrintingConfig<TOwner> parent)
{
    public TypePrintingConfig<TOwner, TType> SetCulture(CultureInfo culture)
    {
        rules.AddRule(new CultureRule(culture, typeof(TType)));
        return this;
    }

    public TypePrintingConfig<TOwner, TType> Serialize(Func<TType, string> serializer)
    {
        rules.AddRule(new SerializationRule<TType>(serializer));
        return this;
    }

    public TypePrintingConfig<TOwner, TType> Exclude()
    {
        rules.AddRule(new ExcludeRule(typeof(TType)));
        return this;
    }

    public TypePrintingConfig<TOwner, TType> Trim(int length)
    {
        if (typeof(TType) != typeof(string))
            throw new InvalidOperationException("Trim доступен только для строк.");

        rules.AddRule(new TrimStringRule(typeof(TType), length));
        return this;
    }


    public PrintingConfig<TOwner> Apply() => parent;
}