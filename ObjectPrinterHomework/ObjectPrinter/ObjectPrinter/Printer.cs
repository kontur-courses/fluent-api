namespace ObjectPrinter.ObjectPrinter
{
    public class Printer
    {
        public static PrintingConfig<T> For<T>()
        {
            return new();
        }
    }
}