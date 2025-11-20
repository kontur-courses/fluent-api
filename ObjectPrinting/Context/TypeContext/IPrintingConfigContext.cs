using System;

namespace ObjectPrinting.Context;

public interface IPrintingConfigContext<TOwner, T>
{
    IPrintingConfigContext<TOwner, T> SetSerializer(Func<T, string> serializer);
    IPrintingConfigContext<TOwner, T> UsingCulture(IFormatProvider culture);
    IPrintingConfigContext<TOwner, T> ExcludeType();
    PrintingConfig<TOwner> End();
}