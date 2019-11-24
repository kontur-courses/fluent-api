using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    interface IPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> AddCustomPrint<TProperty>(Func<TProperty, string> func);
    }
}
