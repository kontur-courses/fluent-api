using System;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    internal interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        Expression<Func<TOwner, TPropType>> MemberSelector { get; }
    }
}