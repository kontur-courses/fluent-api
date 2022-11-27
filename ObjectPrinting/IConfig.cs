using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        PropertyInfo Properties { get; set; }
    }
}
