using System.Reflection;
using ObjectSerializer.Interfaces;

namespace ObjectSerializer.ConcreteConfigs;

public class FieldPrintConfig<TOwner, TField> : IFieldPrintConfig<TOwner, TField>
{
    private readonly MemberInfo memberInfo;
    private readonly PrintingConfig<TOwner> printingConfig;

    public FieldPrintConfig(PrintingConfig<TOwner> printingConfig, MemberInfo memberInfo)
    {
        this.printingConfig = printingConfig;
        this.memberInfo = memberInfo;
    }

    public IPrintingConfig<TOwner> TrimToLength(int length)
    {
        if (length <= 0)
            throw new ArgumentException("Длина обрезаемой строки не может быть <= 0!");

        printingConfig.FieldLengths.Add(memberInfo, (0, length));

        return printingConfig;
    }

    public IPrintingConfig<TOwner> Trim(int start, int length)
    {
        if (start < 0 || length <= 0)
            throw new ArgumentException($"Неправильно задан диапозон обрезания строки:" +
                                        $" start = {start} должен >= 0, length = {length} > 0!");

        printingConfig.FieldLengths.Add(memberInfo, (start, length));

        return printingConfig;
    }

    public IPrintingConfig<TOwner> Using(Func<TField, string> printFieldFunc)
    {
        AddFuncInStorage(f => printFieldFunc((TField)f));

        return printingConfig;
    }

    private void AddFuncInStorage(Func<object, string> func)
    {
        printingConfig.MemberInfosPrintFunc[memberInfo] = func;
    }
}