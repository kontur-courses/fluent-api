using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public interface ICustomSerializablePrintingConfig
    {
        Dictionary<Type, Delegate> TypeSerializers { get; }
        Dictionary<MemberInfo, Delegate> PropertySerializers { get; }
    }
}