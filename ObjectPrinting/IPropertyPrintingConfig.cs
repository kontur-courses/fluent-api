namespace ObjectPrinting
{
    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        Configurator<TOwner> ParentConfig { get; }
    }
}