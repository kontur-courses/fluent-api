using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting.PrintingConfigs;

public class PrintingConfig<TOwner>
{
    private int maxRecursionDepth = 16;
    private readonly HashSet<Type> excludedTypes = new();
    private readonly HashSet<MemberInfo> excludedMembers = new();
    private readonly Dictionary<Type, Func<object, string>> typeSerializers = new();
    private readonly Dictionary<MemberInfo, Func<object, string>> memberSerializers = new();
    private readonly Dictionary<Type, CultureInfo> typeCultures = new();
    private readonly Dictionary<MemberInfo, int> trimLengths = new();

    public PrintingConfig<TOwner> SetMaxRecursionDepth(int recursionDepth)
    {
        if (recursionDepth < 0)
        {
            throw new ArgumentException("Max recursion depth must be greater than zero.");
        }
        maxRecursionDepth = recursionDepth;
        return this;
    }
    
    public PrintingConfig<TOwner> Excluding<TProp>()
    {
        excludedTypes.Add(typeof(TProp));
        return this;
    }

    public PrintingConfig<TOwner> Excluding<TProp>(Expression<Func<TOwner, TProp>> selector)
    {
        excludedMembers.Add(GetMember(selector));
        return this;
    }

    public TypePrintingConfig<TOwner, TProp> Printing<TProp>()
    {
        return new TypePrintingConfig<TOwner, TProp>(this);
    }

    public PropertyPrintingConfig<TOwner, TProp> Printing<TProp>(Expression<Func<TOwner, TProp>> selector)
    {
        return new PropertyPrintingConfig<TOwner, TProp>(this, selector);
    }

    public StringPropertyPrintingConfig<TOwner> Printing(Expression<Func<TOwner, string>> selector)
    {
        return new StringPropertyPrintingConfig<TOwner>(this, selector);
    }
    
    public PrintingConfig<TOwner> SetSerializerFor<TProp>(Func<TProp, string> serializer)
    {
        typeSerializers[typeof(TProp)] = p => serializer((TProp)p);
        return this;
    }
    
    public PrintingConfig<TOwner> SetSerializerFor<TProp>(
        Expression<Func<TOwner, TProp>> selector, Func<TProp, string> serializer)
    {
        memberSerializers[GetMember(selector)] = m => serializer((TProp)m);
        return this;
    }

    public PrintingConfig<TOwner> SetCultureFor<TProp>(CultureInfo culture)
        where TProp : IFormattable
    {
        typeCultures[typeof(TProp)] = culture;
        return this;
    }

    public PrintingConfig<TOwner> TrimMember(Expression<Func<TOwner, string>> selector, int trimLength)
    {
        if (trimLength < 0)
        {
            throw new ArgumentException("Max length cannot be less than zero.");
        }
        
        trimLengths[GetMember(selector)] = trimLength;
        return this;
    }

    private PrinterSettings CreateSettings()
    {
        return new PrinterSettings(
            maxRecursionDepth,
            new HashSet<Type>(excludedTypes),
            new HashSet<MemberInfo>(excludedMembers),
            new Dictionary<Type, Func<object, string>>(typeSerializers),
            new Dictionary<MemberInfo, Func<object, string>>(memberSerializers),
            new Dictionary<Type, CultureInfo>(typeCultures),
            new Dictionary<MemberInfo, int>(trimLengths)
        );
    }

    public ObjectPrinter Create()
    {
        return new ObjectPrinter(CreateSettings());
    }
    
    private static MemberInfo GetMember<TProp>(Expression<Func<TOwner, TProp>> selector)
    {
        if (selector.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member;
        }

        throw new ArgumentException("Selector must refer to a property or a field.");
    }
}