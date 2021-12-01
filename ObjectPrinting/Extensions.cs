namespace ObjectPrinting
{
    public static class Extensions
    {
        public static BasicTypeConfig<string, TOwner> WithTrimLength<TOwner>(this BasicTypeConfig<string, TOwner> config, int length)
        {
            config.Container.WithTrimLength(length);
            return config;
        }
    }
}