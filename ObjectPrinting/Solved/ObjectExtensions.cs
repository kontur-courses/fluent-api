namespace ObjectPrinting.Solved;

public static class ObjectExtensions
{
    public static string PrintToString<T>(this T obj)
    {
        var config = obj.GetDefaultPrintingConfig();
        return obj.PrintToString(config);
    }

    public static PrintingConfig<T> GetDefaultPrintingConfig<T>(this T obj)
    {
        return new PrintingConfig<T>(obj);
    }
}