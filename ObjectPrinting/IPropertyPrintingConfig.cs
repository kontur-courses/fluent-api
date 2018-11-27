using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> PrintingConfig { get; }
        Expression<Func<TOwner, TPropType>> PropertySelector { get; }
    }
}