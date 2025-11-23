using System;
using System.Globalization;
using ObjectPrinting.Interface;

namespace ObjectPrinting;

public class MemberPrintingConfig<TOwner, TProp>
{
    private readonly PrintingConfig<TOwner> config;
    private readonly string memberName;

    internal MemberPrintingConfig(PrintingConfig<TOwner> config, string memberName)
    {
        this.config = config;
        this.memberName = memberName;
    }
    
    public PrintingConfig<TOwner> Using(Func<TProp, string> serializer)
    {
        config.PropertySerializers[memberName] = obj => serializer((TProp)obj);
        return config;
    }
    
    public IStringMemberConfig<TOwner> AsString()
    {
        return typeof(TProp) == typeof(string)
            ? new StringMemberConfig<TOwner>(config, memberName)
            : throw new InvalidOperationException("Property is not string");
    }
}
