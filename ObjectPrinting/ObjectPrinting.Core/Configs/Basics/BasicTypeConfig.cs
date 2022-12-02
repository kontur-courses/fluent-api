namespace ObjectPrinting.Core.Configs.Basics
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
            throw new NotImplementedException();
        }

        public BasicTypeConfig<T, TOwner> WithSerializer(Func<T, string> serializer)
        {
            throw new NotImplementedException();
        }
    }
}