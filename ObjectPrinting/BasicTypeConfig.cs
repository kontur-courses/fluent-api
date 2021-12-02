using System;

namespace ObjectPrinting
{
    public class BasicTypeConfig<T, TOwner>
    {
        internal readonly PrintingConfig<TOwner> Container;

        internal BasicTypeConfig(PrintingConfig<TOwner> container)
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