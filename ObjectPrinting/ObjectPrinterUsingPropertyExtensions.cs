using System;

namespace ObjectPrinting.Tests;

public static class ObjectPrinterUsingPropertyExtensions
{
    public static PrintingConfig<TOwner> Trim<TOwner>(this ObjectPrinterUsingProperty<TOwner, string> property, int length)
    {
        property.PrintingConfig.SerializationsByPropertyFullName[property.PropertyFullName] = obj => ((string)obj).Substring(0, Math.Min(length, ((string)obj).Length));
        return property.PrintingConfig;
    }
}