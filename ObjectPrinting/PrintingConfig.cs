using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Solved;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private readonly ObjectSerializer _objectSerializer = new();

    public string PrintToString(TOwner obj)
    {
        return _objectSerializer.SerializeObject(obj);
    }

    public PrintingConfig<TOwner> Exclude<T>()
    {
        _objectSerializer.AddExcludedType(typeof(T));
        return this;
    }

    public PrintingConfig<TOwner> Exclude(Expression<Func<TOwner, object>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);

        _objectSerializer.AddExcludedMember(selector.GetMemberInfo());
        return this;
    }

    public PrintingConfig<TOwner> SetModifiedSerialization<TMember>
        (Expression<Func<TOwner, TMember>> memberSelector, Func<TMember, string> serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);

        var memberInfo = memberSelector.GetMemberInfo();
        _objectSerializer.AddModifiedMemberSerializer(memberInfo, serializer);
        return this;
    }

    public PrintingConfig<TOwner> SetTypeSerialization<TMember>(Func<TMember, string> serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);

        _objectSerializer.AddModifiedTypeSerializer<TMember>(serializer);
        return this;
    }

    public PrintingConfig<TOwner> SetNumericCulture(CultureInfo cultureInfo)
    {
        ArgumentNullException.ThrowIfNull(cultureInfo);

        _objectSerializer.SetNumericCulture(cultureInfo);
        return this;
    }

    public PrintingConfig<TOwner> Truncate(Expression<Func<TOwner, string>> selector, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        return SetModifiedSerialization(
            selector,
            value => value[..Math.Min(length, value.Length)]);
    }
}