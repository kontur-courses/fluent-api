using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    interface IPrintingConfig<TOwner>
    {
        Dictionary<Type, Delegate> TypeSerializationFormats { get; }
        Dictionary<PropertyInfo, Delegate> PropertySerializationFormats { get; }
    }
}
