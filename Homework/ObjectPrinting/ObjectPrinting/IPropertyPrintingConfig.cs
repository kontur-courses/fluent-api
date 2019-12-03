namespace ObjectPrinting
{
    // ReSharper disable once UnusedTypeParameter
    internal interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}