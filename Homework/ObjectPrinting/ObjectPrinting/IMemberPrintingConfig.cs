namespace ObjectPrinting
{
    // ReSharper disable once UnusedTypeParameter
    internal interface IMemberPrintingConfig<TOwner, TMemberType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}