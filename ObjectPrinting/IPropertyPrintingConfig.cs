using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    interface IPropertyPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        PropertyInfo PropertyInfo { get; }
    }
}
