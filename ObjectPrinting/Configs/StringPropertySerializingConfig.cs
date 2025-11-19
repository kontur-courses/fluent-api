namespace ObjectPrinting;

public class StringPropertySerializingConfig<TOwner> : PropertySerializingConfig<TOwner, string>
{
    public StringPropertySerializingConfig(PrintingConfig<TOwner> printingConfig, string propertyName) 
        : base(printingConfig, propertyName) { }

    public PrintingConfig<TOwner> TrimTo(int maxLength)
    {
        return Use(s => s?.Length <= maxLength ? s : s.Substring(0, maxLength));
    }
}