using System;

namespace ObjectPrinting;

public interface IPropertySerializer<TOwner, TProperty>
{
    public PrintingConfig<TOwner> Use(Func<TProperty, string> converter);
}