using ObjectPrinter.ObjectPrinter;

namespace ObjectPrinter.Interfaces
{
    public interface IMemberConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}