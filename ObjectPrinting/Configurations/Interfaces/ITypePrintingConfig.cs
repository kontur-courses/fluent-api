using System;
using System.Globalization;

namespace ObjectPrinting.Configurations.Interfaces;

public interface ITypePrintingConfig<TOwner>
{
    public PrintingConfig<TOwner> ParentConfig { get; }
    public Func<object, string>? Serializer { get; } 
    public CultureInfo? CultureInfo { get; }
}