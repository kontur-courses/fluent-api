using System;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropType>(
    PrintingConfig<TOwner> printingConfig, 
    string? memberPath)
{
    private readonly PrintingConfig<TOwner> printingConfig = printingConfig;

    private readonly string? memberPath = memberPath;

    public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
    {
        Func<object, string> serializer = obj => print((TPropType)obj);
        if (memberPath is null)
        {
            printingConfig.AddTypeSerializer(typeof(TPropType), serializer);
        }
        else
        {
            printingConfig.AddMemberSerializer(memberPath, serializer);
        }

        return printingConfig;
    }
}