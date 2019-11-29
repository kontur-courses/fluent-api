using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public interface IPrintingConfig
    {
        Dictionary<Type, Func<object, string>> typeSerialisation { get; }
        Dictionary<MemberInfo, Func<object,string>> propertySerialisation { get; }
    }
}