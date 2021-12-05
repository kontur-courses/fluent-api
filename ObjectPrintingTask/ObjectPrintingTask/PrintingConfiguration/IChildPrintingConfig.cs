using System;

namespace ObjectPrintingTask.PrintingConfiguration
{
    public interface IChildPrintingConfig<TOwner, TMemberType>
    {
        Printer<TOwner> Using(Func<TMemberType, string> scenario);
    }
}