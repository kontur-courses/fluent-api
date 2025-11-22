using System;
using ObjectPrinting.Configs.Interfaces;

namespace ObjectPrinting.Configs;

public class TypePrintingConfig<TOwner, TPropType> : IChildPrintingConfig<TOwner, TPropType>
{
    private readonly PrintingConfig<TOwner> printingConfig;

    PrintingConfig<TOwner> IChildPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;

    internal TypePrintingConfig(PrintingConfig<TOwner> parentConfig)
    {
        printingConfig = parentConfig ?? throw new ArgumentNullException(nameof(parentConfig));
    }

    public PrintingConfig<TOwner> Use(Func<TPropType, string> serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);
        printingConfig.Settings.TypeSerializers[typeof(TPropType)] = obj => serializer((TPropType)obj!);
        return printingConfig;
    }

    public PrintingConfig<TOwner> Exclude()
    {
        printingConfig.Settings.ExcludedTypes.Add(typeof(TPropType));
        return printingConfig;
    }
}