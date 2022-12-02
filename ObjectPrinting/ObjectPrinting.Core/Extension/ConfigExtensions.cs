using ObjectPrinting.Core.Configs.Basics;

namespace ObjectPrinting.Core.Extension
{
    public static class ConfigExtensions
    {
        public static BasicTypeConfig<string, TOwner> TrimEnd<TOwner>(this BasicTypeConfig<string, TOwner> config, int length)
        {
            config.Container.TrimEnd(length);
            return config;
        }
    }
}
