using System;

namespace ObjectPrintingTask.PrintingConfiguration
{
    public interface IChildPrintingConfig<TOwner, TMemberType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }

        PrintingConfig<TOwner> Using(Func<TMemberType, string> scenario);
    }
}