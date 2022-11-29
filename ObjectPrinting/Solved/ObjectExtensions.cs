namespace ObjectPrinting.Solved;

public static class ObjectExtensions
{
    public static string PrintToString<T>(this T obj)
    {
        var config = ObjectPrinter.For<T>();
        return obj.PrintToString(config);
    }
}