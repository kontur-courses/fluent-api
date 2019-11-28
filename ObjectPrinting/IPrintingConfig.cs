using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public interface IPrintingConfig
    {
        Dictionary<Type, Delegate> typeSerialisation { get; }
        Dictionary<MemberInfo, Delegate> propertySerialisation { get; }
    }
}