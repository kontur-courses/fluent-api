using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
{
    PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    private readonly PrintingConfig<TOwner> printingConfig;
    private readonly Dictionary<Type, Delegate> serializersForTypes;
    private readonly Dictionary<MemberInfo, Delegate> serializersForMembers;
    private readonly MemberInfo propertyInfo;

    public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig,
        Dictionary<Type, Delegate> serializersForTypes,
        Dictionary<MemberInfo, Delegate> serializersForMembers,
        Expression<Func<TOwner, TPropType>> memberSelector = null)
    {
        this.printingConfig = printingConfig;
        this.serializersForTypes = serializersForTypes;
        this.serializersForMembers = serializersForMembers;
        propertyInfo = ((MemberExpression)memberSelector?.Body)?.Member;
    }

    public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
    {
        if (propertyInfo is null)
            serializersForTypes[typeof(TPropType)] = print;
        else
            serializersForMembers[propertyInfo] = print;

        return printingConfig;
    }
}