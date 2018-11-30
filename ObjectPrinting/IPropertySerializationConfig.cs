namespace ObjectPrinting
{
    interface IPropertySerializationConfig<TOwner>
    {
        PrintingConfig<TOwner> PrintingConfig { get; }
        string PropertyName { get; }
    }
}
