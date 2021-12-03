using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public interface IMemberPrintingConfig<TOwner, TMember>
    {
        public IPrintingConfig<TOwner> Using(Func<TMember, string> alternativeSerializer);
        
        public IPrintingConfig<TOwner> Using(Expression<Func<TOwner, TMember>> memberSelector,
            Func<TMember, string> alternativeSerializer);
    }
}