using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    interface IPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> AddCustomPrintForType<TProperty>(Func<TProperty, string> func);
        PrintingConfig<TOwner> AddCustomPrintForProperty<TProperty>(Func<TProperty, string> func, PropertyInfo property);
    }
}
