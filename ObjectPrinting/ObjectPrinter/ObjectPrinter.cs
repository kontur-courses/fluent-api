namespace ObjectPrinting.ObjectPrinter;

public static class ObjectPrinter
{
    public static PrintingConfig<T> For<T>()
    {
        return new PrintingConfig<T>();
    }
}