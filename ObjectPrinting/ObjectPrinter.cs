using ObjectPrinting.Configuration;

namespace ObjectPrinting;

public static class ObjectPrinter
{
    public static PrintingConfig<TOwner> For<TOwner>()
    {
        return new PrintingConfig<TOwner>();
    }
}