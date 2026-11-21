using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private readonly PrintingConfigState state;

    public PrintingConfig() : this(PrintingConfigState.Empty)
    {
    }

    private PrintingConfig(PrintingConfigState state)
    {
        this.state = state;
    }

    public PrintingConfig<TOwner> Exclude<TProp>() =>
        new(state with { ExcludedTypes = state.ExcludedTypes.Add(typeof(TProp)) });

    public PrintingConfig<TOwner> Exclude<TProp>(Expression<Func<TOwner, TProp>> memberSelector)
    {
        var member = ExtractMemberInfo(memberSelector);
        return new(state with { ExcludedMembers = state.ExcludedMembers.Add(member) });
    }

    public MemberPrintingConfig<TOwner, TProp> For<TProp>() => new(this);

    public MemberPrintingConfig<TOwner, TProp> For<TProp>(Expression<Func<TOwner, TProp>> memberSelector)
    {
        var member = ExtractMemberInfo(memberSelector);
        return new(this, member);
    }

    public string PrintToString(TOwner obj) =>
        new ObjectSerializer(state).Serialize(obj);

    internal PrintingConfig<TOwner> WithTypeSerializer(Type type, Func<object, string> serializer) =>
        new(state with { TypeSerializers = state.TypeSerializers.SetItem(type, serializer) });

    internal PrintingConfig<TOwner> WithMemberSerializer(MemberInfo member, Func<object, string> serializer) =>
        new(state with { MemberSerializers = state.MemberSerializers.SetItem(member, serializer) });

    internal PrintingConfig<TOwner> WithTypeCulture(Type type, CultureInfo culture) =>
        new(state with { TypeCultures = state.TypeCultures.SetItem(type, culture) });

    internal PrintingConfig<TOwner> WithMemberCulture(MemberInfo member, CultureInfo culture) =>
        new(state with { MemberCultures = state.MemberCultures.SetItem(member, culture) });

    internal PrintingConfig<TOwner> WithStringMaxLength(int maxLength) =>
        new(state with { StringMaxLength = maxLength });

    internal PrintingConfig<TOwner> WithMemberMaxLength(MemberInfo member, int maxLength) =>
        new(state with { MemberMaxLengths = state.MemberMaxLengths.SetItem(member, maxLength) });

    private static MemberInfo ExtractMemberInfo<TProp>(Expression<Func<TOwner, TProp>> memberSelector)
    {
        ArgumentNullException.ThrowIfNull(memberSelector);

        if (memberSelector.Body is MemberExpression { Member: PropertyInfo or FieldInfo } memberExpression)
            return memberExpression.Member;

        throw new ArgumentException("Expression must select a property or field.", nameof(memberSelector));
    }
}
