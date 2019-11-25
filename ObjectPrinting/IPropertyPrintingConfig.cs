using System;
using System.Reflection;

namespace ObjectPrinting
{
    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> PrintingConfig { get; }
        Type Type { get; }
        PropertyInfo Property { get; }
    }
}