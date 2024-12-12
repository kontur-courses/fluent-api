namespace ObjectSerializer.Interfaces;

public interface IFieldPrintConfig<TOwner, TField>
{
    IPrintingConfig<TOwner> TrimToLength(int length);

    IPrintingConfig<TOwner> Trim(int start, int length);

    IPrintingConfig<TOwner> Using(Func<TField, string> printFieldFunc);
}