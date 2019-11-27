namespace ObjectPrinting
{
    interface IPropertyPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        string PropertyFullName { get; }
    }
}
