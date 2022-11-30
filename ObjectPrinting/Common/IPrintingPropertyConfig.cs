namespace ObjectPrinting.Common
{
    public interface IPrintingPropertyConfig<TOwner, T> : IPrintingConfig<TOwner>, IPrintingPropertyBaseConfig<TOwner, T>
    { }
}