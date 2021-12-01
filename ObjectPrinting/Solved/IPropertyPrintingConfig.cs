using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting.Solved
{
    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }
}
