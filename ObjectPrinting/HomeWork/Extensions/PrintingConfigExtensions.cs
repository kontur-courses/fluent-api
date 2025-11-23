using ObjectPrinting.HomeWork.PrintUtils;

namespace ObjectPrinting.HomeWork.Extensions;

public static class PrintingConfigExtensions
{
    public static string PrintToString<T>(this T obj, Func<PrintingConfig<T>, PrintingConfig<T>> config)
    {
        return config(ObjectPrinter.For<T>()).PrintToString(obj);
    }
}