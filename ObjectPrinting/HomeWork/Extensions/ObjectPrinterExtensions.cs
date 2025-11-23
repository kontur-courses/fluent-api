namespace ObjectPrinting.HomeWork.Extensions;

public static class ObjectPrinterExtensions
{
    public static string PrintToString<T>(this T obj)
    {
        return ObjectPrinter.For<T>().PrintToString(obj);
    }
}