using System;
using System.Globalization;

namespace ObjectPrinting;

public interface ITypePrintingConfig<TOwner, TType>
{
    PrintingConfig<TOwner> ParentConfig { get; }
}
