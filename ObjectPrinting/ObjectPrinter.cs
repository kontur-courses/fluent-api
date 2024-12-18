using System;

namespace ObjectPrinting;

public static class ObjectPrinter
{
    public static PrintingConfig<T> For<T>()
    {
        return new PrintingConfig<T>();
    }
    
    public static string Serialize<TSerialize>(TSerialize obj) =>
        new PrintingConfig<TSerialize>().PrintToString(obj);

    public static string Serialize<TSerialize>(TSerialize obj,
        Func<PrintingConfig<TSerialize>, PrintingConfig<TSerialize>> config)
        => config(For<TSerialize>()).PrintToString(obj);
}