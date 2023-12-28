namespace ObjectPrinting.Extensions;

public static class ObjectExtensions
{
    public static string PrintToString<T>(this T obj, PrintingConfig<T> printingConfig)
    {
        return new Serializer<T>(printingConfig).PrintToString(obj);
    }
    
    public static string PrintToString<T>(this T obj)
    {
        return new Serializer<T>(ObjectPrinter.For<T>()).PrintToString(obj);
    }
}