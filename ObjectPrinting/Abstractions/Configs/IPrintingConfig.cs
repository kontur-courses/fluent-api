using System;
using System.Linq.Expressions;

namespace ObjectPrinting.Abstractions.Configs;

public interface IPrintingConfig<TOwner>
{
    public string PrintToString(TOwner? obj);

    public IPrintingConfig<TOwner> Exclude<TMember>();

    public IPrintingConfig<TOwner> Exclude<TMember>(Expression<Func<TOwner, TMember>> selector);

    public IMemberPrintingConfig<TOwner, TMember> Printing<TMember>();

    public IMemberPrintingConfig<TOwner, TMember> Printing<TMember>(Expression<Func<TOwner, TMember>> selector);
}