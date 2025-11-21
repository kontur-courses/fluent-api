using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Solved;

public class PrintingConfig<TOwner>
{
    internal PrintingSettings Settings = new();
        
    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this, null);
    }

    public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        return new PropertyPrintingConfig<TOwner, TPropType>(this, GetMemberInfo(memberSelector));
    }

    public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        var member = GetMemberInfo(memberSelector);
        Settings.ExcludedMembers.Add(member);
        return this;
    }

    public PrintingConfig<TOwner> Excluding<TPropType>()
    {
        Settings.ExcludedTypes.Add(typeof(TPropType));
        return this;
    }

    public string PrintToString(TOwner obj)
    {
        var printer = new Printer(Settings);
        return printer.PrintToString(obj, 0, null);
    }
    
    private MemberInfo GetMemberInfo<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
    {
        if (memberSelector.Body is MemberExpression memberExpression)
            return memberExpression.Member;
        throw new ArgumentException();
    }
        
    internal void AddTypeSerializer<TPropType>(Func<TPropType, string> serialize)
    {
        Settings.TypeSerializers[typeof(TPropType)] = o => serialize((TPropType)o);
    }

    internal void AddMemberSerializer<TPropType>(MemberInfo? member, Func<TPropType, string?> serialize)
    {
        ArgumentNullException.ThrowIfNull(member);
        Settings.MemberSerializers[member] = o => serialize((TPropType)o);
    }

    internal void AddTypeCulture<TPropType>(CultureInfo culture)
    {
        Settings.TypeCultures[typeof(TPropType)] = culture;
    }

    internal void AddMemberCulture(MemberInfo member, CultureInfo culture)
    {
        if (member == null)
            throw new ArgumentNullException(nameof(member));

        Settings.MemberCultures[member] = culture;
    }

    internal void AddStringTrimming(MemberInfo? member, int maxLen)
    {
        AddMemberSerializer<string>(member, s => s.Length <= maxLen ? s : s.Substring(0, maxLen));
    }
}