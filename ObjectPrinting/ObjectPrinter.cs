using ObjectPrinting.Configs;

namespace ObjectPrinting;

public class ObjectPrinter
{
    public static PrintingConfig<T> InClass<T>()
    {
        return new PrintingConfig<T>(new PrintingSettings());
    }
}