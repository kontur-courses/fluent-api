namespace ObjectPrinting;

public static class Extensions
{
    public static PrintingConfig<TOwner>.BasicTypeConfig<string> WithTrimLength<TOwner>(this PrintingConfig<TOwner>.BasicTypeConfig<string> config, int length)
    {
        config.Container.WithTrimLength(length);
        return config;
    }
}
