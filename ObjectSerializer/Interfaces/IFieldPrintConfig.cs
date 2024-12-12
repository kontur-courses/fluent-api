namespace ObjectSerializer.Interfaces;

public interface IFieldPrintConfig<TOwner, TField>
{
    IPrintingConfig<TOwner> TrimToLength(uint length);

    IPrintingConfig<TOwner> Using(Func<TField, string> printFieldFunc);
}