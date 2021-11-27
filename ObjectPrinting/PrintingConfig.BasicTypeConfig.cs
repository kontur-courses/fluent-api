using System;

namespace ObjectPrinting;

public partial class PrintingConfig<TOwner>
{

    public class BasicTypeConfig<T>
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

        public BasicTypeConfig<T> WithSerializer(Func<T, string> serializer)
        {
            Container.WithSerializer(serializer);
            return this;
        }
    }
}
