using System;
using System.Globalization;
using ObjectPrinting.ObjectConfiguration;

namespace ObjectPrinting.MemberConfigurator;

public interface IMemberConfigurator<TOwner, T>
{
    IObjectConfiguration<TOwner> Configure(Func<string, string> func);
}