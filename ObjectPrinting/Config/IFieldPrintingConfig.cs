using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public interface IFieldPrintingConfig<TOwner, TPropType> : IConfig<TOwner, TPropType>
    {
        FieldInfo FieldInfo { get; }
    }
}
