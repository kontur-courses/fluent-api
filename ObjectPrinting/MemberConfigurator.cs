using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Reflection;

namespace ObjectPrinting;

public class MemberConfigurator<TOwner, T> : IMemberConfigurator<TOwner, T>
{
    private MemberInfo memberInfo;
    
    public IBasicConfigurator<TOwner> BasicConfigurator { get; }
    public IBasicConfigurator<TOwner> Configure(CultureInfo cultureInfo)
    {
        var memberConfig = new MemberConfig(cultureInfo, 0);
        BasicConfigurator.Dict.Add(memberInfo, memberConfig);

        return BasicConfigurator;
    }

    public IBasicConfigurator<TOwner> Configure(int length)
    {
         var memberConfig = new MemberConfig(CultureInfo.CurrentCulture, length);
         BasicConfigurator.Dict.Add(memberInfo, memberConfig);

         return BasicConfigurator;
    }

    public MemberConfigurator(IBasicConfigurator<TOwner> basicConfigurator, MemberInfo memberInfo)
    {
        BasicConfigurator = basicConfigurator;
        this.memberInfo = memberInfo;
    }
}