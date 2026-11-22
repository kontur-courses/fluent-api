using System;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting;

public class PropertyConfig<TOwner, TProperty>(
    PrintingConfig<TOwner> printingConfig, 
    ObjectSerializer objectSerializer,
    Expression<Func<TOwner, TProperty>> selector)
{

}