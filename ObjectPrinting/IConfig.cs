namespace ObjectPrinting
{
    public interface IConfig<TOwner, TProp>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}
