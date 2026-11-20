namespace ObjectPrinting;

public static class ObjectPrinterExtensions
{
    public static string PrintToString<TOwner>(this TOwner owner)
    {
        return ObjectPrinter.For<TOwner>().PrintToString(owner);
    }
}
