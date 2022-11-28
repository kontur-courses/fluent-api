using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private readonly Type[] finalTypes =
    {
        typeof(int), typeof(double), typeof(float), typeof(string),
        typeof(DateTime), typeof(TimeSpan)
    };

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this);
    }

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
        Expression<Func<TOwner, TPropType>> memberSelector)
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this);
    }

    public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        return this;
    }

    public PrintingConfig<TOwner> Excluding<TPropType>()
    {
        return this;
    }

    public string PrintToString(TOwner obj)
    {
        return PrintToString(obj, 0);
    }

    private string PrintToString(object? obj, int nestingLevel)
    {
        if (obj is null)
            return "null";

        if (finalTypes.Contains(obj.GetType()))
            return $"{obj}";

        var sb = new StringBuilder();
        
        var type = obj.GetType();
        sb.AppendLine(type.Name);
        
        var identation = new string('\t', nestingLevel + 1);
        foreach (var property in type.GetProperties())
        {
            var print = PrintToString(property.GetValue(obj), nestingLevel + 1);
            sb.AppendLine($"{identation}{property.Name} = {print}");
        }
        
        return sb.ToString().Trim();
    }
}