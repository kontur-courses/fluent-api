using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting;

public class PropertyConfig<TOwner, TProperty>(
    PrintingConfig<TOwner> printingConfig,
    ObjectSerializer objectSerializer,
    Expression<Func<TOwner, TProperty>> selector)
{
    internal ObjectSerializer ObjectSerializer { get; } = objectSerializer;

    internal MemberInfo MemberInfo { get; } = selector.GetMemberInfo();

    internal PrintingConfig<TOwner> PrintingConfig { get; } = printingConfig;
}