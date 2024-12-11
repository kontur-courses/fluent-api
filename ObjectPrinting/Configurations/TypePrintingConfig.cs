using System;
using System.Globalization;

namespace ObjectPrinting.Configurations;

public class TypePrintingConfig<TOwner, TType>(PrintingConfig<TOwner> printingConfig)
{
    public PrintingConfig<TOwner> ParentConfig { get; } = printingConfig;

    public PrintingConfig<TOwner> Using(Func<TType, string> print)
    {
        return ParentConfig.AddTypeSerialize(print);
    }

    internal PrintingConfig<TOwner> Using<TFType>(CultureInfo cultureInfo)
        where TFType : IFormattable, TType
    {
        return ParentConfig.SetTypeCulture<TFType>(cultureInfo);
    }
}