namespace ObjectPrinting;

public class ObjectPrinter
{
    public static PrintingConfig<T> For<T>()
    {
        return new PrintingConfig<T>();
    }

    public static string Print<T>(T obj)
    {
        return For<T>().PrintToString(obj);
    }
}
