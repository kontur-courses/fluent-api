using System;
using System.Globalization;

namespace ObjectPrinting.Solved.Interfaces
{
    public interface IPrinting<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}