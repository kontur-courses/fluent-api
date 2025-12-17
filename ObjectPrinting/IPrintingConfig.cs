using System;
using System.Linq.Expressions;

namespace ObjectPrinting;

public interface IPrintingConfig<TOwner>
{
    IPrintingConfig<TOwner> Excluding<TPropType>();

    IPrintingConfig<TOwner> Excluding<TProp>(Expression<Func<TOwner, TProp>> memberSelector);

    IPropertyPrintingConfig<TOwner, TProp> Printing<TProp>();

    IPropertyPrintingConfig<TOwner, TProp> Printing<TProp>(Expression<Func<TOwner, TProp>> memberSelector);

    string PrintToString(TOwner obj);
}