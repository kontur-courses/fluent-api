using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    interface IPrintingConfig
    {
        Dictionary<Type, Delegate> СustomPrints { get; }
    }
}
