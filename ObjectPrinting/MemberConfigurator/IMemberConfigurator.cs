using System;
using ObjectPrinting.ObjectConfiguration;

namespace ObjectPrinting.MemberConfigurator;

public interface IMemberConfigurator<TOwner, T>
{
    IObjectConfiguration<TOwner> Configure(Func<object, string> func);
}