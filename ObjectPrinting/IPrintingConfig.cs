using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    interface IPrintingConfig<TOwner>
    {
        void AddPropertySerializationFormat(PropertyInfo property, Delegate format);
        void AddTypeSerializationFormat(Type type, Delegate format);
    }
}
