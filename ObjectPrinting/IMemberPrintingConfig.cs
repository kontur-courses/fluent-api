using System;
using System.Linq.Expressions;

namespace ObjectPrinting;

public interface IMemberPrintingConfig<TOwner, TFieldType>
{
    IPrintingConfig<TOwner> Config { get; }

    IPrintingConfig<TOwner> With(Func<TFieldType, string> serializer);
    IPrintingConfig<TOwner> With(Expression<Func<TOwner, TFieldType>> expression, Func<TFieldType, string> serializer);
}