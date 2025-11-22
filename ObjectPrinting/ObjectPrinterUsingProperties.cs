namespace ObjectPrinting;

public class ObjectPrinterUsingProperties<TOwner, T>
{
    private PrintingConfig<TOwner> printingConfig;

    public PrintingConfig<TOwner> PrintingConfig => printingConfig;

    public ObjectPrinterUsingProperties(PrintingConfig<TOwner> printingConfig)
    {
        this.printingConfig = printingConfig;
    }
}