using System;
using ObjectPrinting.ObjectConfiguration;

namespace ObjectPrinting.MemberConfigurator;

public interface IMemberConfigurator<TOwner, out T>
{
    IObjectConfiguration<TOwner> Configure(Func<T, string> func);
}