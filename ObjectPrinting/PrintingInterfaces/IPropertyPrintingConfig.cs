using System;
using System.Reflection;

namespace ObjectPrinting.PrintingInterfaces
{
    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        PropertyInfo Property { get; }
        Type Type { get; }
    }
}