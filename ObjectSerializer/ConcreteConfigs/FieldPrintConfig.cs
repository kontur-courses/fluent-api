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
        AddFuncInStorage(obj => obj.ToString()
            .Substring(0, length));

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