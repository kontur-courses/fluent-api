using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public interface IPrintingConfig<TOwner>
    {
        public IPrintingConfig<TOwner> Exclude<TExcluding>();

        public IPrintingConfig<TOwner> Exclude<TExcluding>(Expression<Func<TOwner, TExcluding>> memberSelector);

        public IMemberPrintingConfig<TOwner, TMember> Printing<TMember>();

        public string PrintToString(TOwner obj);

        public IPrintingConfig<TOwner> ThrowIfCyclicReferences();
    }
}