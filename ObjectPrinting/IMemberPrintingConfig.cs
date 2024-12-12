using System;
using System.Linq.Expressions;

namespace ObjectPrinting;

public interface IMemberPrintingConfig<TOwner, out TFieldType>
{
    IPrintingConfig<TOwner> Config { get; }

    IPrintingConfig<TOwner> With(Func<TFieldType, string> serializer);
}