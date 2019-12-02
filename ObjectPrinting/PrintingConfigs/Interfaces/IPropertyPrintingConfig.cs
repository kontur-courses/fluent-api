using System;
using System.Reflection;

namespace ObjectPrinting.PrintingConfigs
{
    internal interface IPropertyPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> Config { get; }
        Func<object, PropertyInfo, bool> Filter { get; }
    }
}