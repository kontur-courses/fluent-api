using System;

namespace ObjectPrinting.Configurations.Interfaces;

public interface ITypePrintingConfig<TOwner>
{
    public PrintingConfig<TOwner> ParentConfig { get; }
    public Func<object, string> Serializer { get; }
}