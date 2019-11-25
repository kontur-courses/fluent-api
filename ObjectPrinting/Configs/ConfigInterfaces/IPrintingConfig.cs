using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting.Configs.ConfigInterfaces
{
    internal interface IPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> AddExcludedType(Type type);
        PrintingConfig<TOwner> AddTypeSerialization(Type type, Func<object, string> serialization);
        PrintingConfig<TOwner> AddPropertySerialization(PropertyInfo propertyInfo, Func<object, string> serialization);
        PrintingConfig<TOwner> AddExcludedProperty(PropertyInfo propertyInfo);
        PrintingConfig<TOwner> SetNestingLevel(int nestingLevel);
    }
}