namespace ObjectPrinting.Solved.PrintingConfiguration
{
    public interface IMemberPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}