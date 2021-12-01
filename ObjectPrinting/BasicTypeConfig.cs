using System;

namespace ObjectPrinting
{
    public class BasicTypeConfig<T, TOwner>
    {
        internal readonly IPrintingConfig<TOwner> Container;

        internal BasicTypeConfig(IPrintingConfig<TOwner> container)
        {
            Container = container;
        }

        public void Exclude()
        {
            Container.Exclude<T>();
        }

        public BasicTypeConfig<T, TOwner> WithSerializer(Func<T, string> serializer)
        {
            Container.WithSerializer(serializer);
            return this;
        }
    }
}