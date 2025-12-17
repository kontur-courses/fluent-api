using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TProp>(PrintingConfig<TOwner> parent, Type targetType, MemberInfo memberInfo) : IPropertyPrintingConfig<TOwner, TProp>
{
    private readonly PrintingConfig<TOwner> _parent = parent;
    private readonly Type _targetType = targetType;
    private readonly MemberInfo? _memberInfo = memberInfo;

    public IPropertyPrintingConfig<TOwner, TProp> UsingSerializer(Func<TProp, string> serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);

        string boxed(object o) => serializer((TProp)o);

        if (_memberInfo != null)
            _parent.SetMemberSerializer(_memberInfo, boxed);
        else
            _parent.SetTypeSerializer(_targetType, boxed);

        return this;
    }

    public IPropertyPrintingConfig<TOwner, TProp> UsingCulture(CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);

        if (_memberInfo != null)
            _parent.SetMemberCulture(_memberInfo, culture);
        else
            _parent.SetTypeCulture(_targetType, culture);

        return this;
    }

    public IPropertyPrintingConfig<TOwner, TProp> TrimmedToLength(int maxLength)
    {
        if (maxLength < 0)
            throw new ArgumentOutOfRangeException(nameof(maxLength), "Параметр maxLength должен быть неотрицательным.");

        if (_memberInfo != null)
            _parent.SetMemberStringTrim(_memberInfo, maxLength);
        else
            _parent.SetTypeStringTrim(_targetType, maxLength);

        return this;
    }

    public IPrintingConfig<TOwner> Excluding<TPropType>()
    {
        return _parent.Excluding<TPropType>();
    }

    public IPrintingConfig<TOwner> Excluding<TAny>(Expression<Func<TOwner, TAny>> memberSelector)
    {
        return _parent.Excluding(memberSelector);
    }

    public IPropertyPrintingConfig<TOwner, TAny> Printing<TAny>()
    {
        return _parent.Printing<TAny>();
    }

    public IPropertyPrintingConfig<TOwner, TAny> Printing<TAny>(Expression<Func<TOwner, TAny>> memberSelector)
    {
        return _parent.Printing(memberSelector);
    }

    public string PrintToString(TOwner obj)
    {
        return _parent.PrintToString(obj);
    }
}
