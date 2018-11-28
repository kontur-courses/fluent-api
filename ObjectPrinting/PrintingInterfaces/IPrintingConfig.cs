using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting.PrintingInterfaces
{
    public interface IPrintingConfig
    {
        Dictionary<Type, Delegate> TypeCustomSerializers { get; }
        Dictionary<PropertyInfo, Delegate> PropertyCustomSerializers { get; }
    }
}