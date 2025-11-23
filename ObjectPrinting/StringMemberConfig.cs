using ObjectPrinting.Interface;

namespace ObjectPrinting;

public class StringMemberConfig<TOwner> : IStringMemberConfig<TOwner>
{
    private readonly PrintingConfig<TOwner> config;
    private readonly string memberName;

    public StringMemberConfig(PrintingConfig<TOwner> config, string memberName)
    {
        this.config = config;
        this.memberName = memberName;
    }

    public PrintingConfig<TOwner> Trim(int maxLength)
    {
        config.TrimLengths[memberName] = maxLength;
        return config;
    }
}
