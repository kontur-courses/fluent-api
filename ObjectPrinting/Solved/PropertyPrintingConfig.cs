using System;

namespace ObjectPrinting.Solved;

public class PropertyPrintingConfig<TOwner, TPropType>
{
    public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig)
    {
        ParentConfig = printingConfig;
        AlternativePrint = null;
    }

    public Func<TPropType, string> AlternativePrint { get; set; }

    public PrintingConfig<TOwner> ParentConfig { get; }

    public string AlternativePrintInvoke(object member)
    {
        return AlternativePrint((TPropType)member);
    }
}