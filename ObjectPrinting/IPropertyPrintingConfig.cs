using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        Expression<Func<TOwner, TPropType>> Member { get; }
    }
}