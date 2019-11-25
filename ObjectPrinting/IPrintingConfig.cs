using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    interface IPrintingConfig
    {
        Dictionary<Type, Func<object, string>> CustomTypesPrints { get; }
        Dictionary<string, Func<object, string>> CustomPropertysPrints { get; }
        int MaxNumberListItems { set; }
    }
}
