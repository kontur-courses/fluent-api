namespace ObjectPrinting;

public interface IPrintingConfig<in TOwner>
{
    string PrintToString(TOwner obj);
}