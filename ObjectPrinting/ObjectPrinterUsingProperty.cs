using System;

namespace ObjectPrinting;

public class ObjectPrinterUsingProperty<TOwner, T>
{
    private PrintingConfig<TOwner> printingConfig;
    private string propertyFullName;

    public PrintingConfig<TOwner> PrintingConfig => printingConfig;

    public string PropertyFullName => propertyFullName;

    public ObjectPrinterUsingProperty(PrintingConfig<TOwner> printingConfig, string propertyFullName)
    {
        this.printingConfig = printingConfig;
        this.propertyFullName = propertyFullName;
    }
    
    public PrintingConfig<TOwner> Serialize(Func<object, string> func)
    {
        printingConfig.SerializationsByPropertyFullName[propertyFullName] = func;
        return printingConfig;
    }
}