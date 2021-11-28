using System;

namespace ObjectPrinting.Solved.PrintingConfiguration
{
    public interface IChildPrintingConfig<TOwner, TMemberType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }

        PrintingConfig<TOwner> Using(Func<TMemberType, string> scenario);
    }
}