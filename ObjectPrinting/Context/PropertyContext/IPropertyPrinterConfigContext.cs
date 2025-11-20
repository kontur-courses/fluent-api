using System;

namespace ObjectPrinting.Context;

public interface IPropertyPrinterConfigContext<TOwner, T>
{
    IPropertyPrinterConfigContext<TOwner, T> SetSerializer(Func<T, string> serializer);
    IPropertyPrinterConfigContext<TOwner, T> ExcludeProperty();
    PrintingConfig<TOwner> End();
}