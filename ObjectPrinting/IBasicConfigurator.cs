using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting;

public interface IBasicConfigurator<TOwner>
{
    IBasicConfigurator<TOwner> Exclude<T>();
    IBasicConfigurator<TOwner> Exclude<T>(Expression<Func<TOwner, T>> expression);
    IMemberConfigurator<TOwner, T> ConfigureProperty<T>(Expression<Func<TOwner, T>> expression);
    PrintingConfig<TOwner> ConfigurePrinter();
    Dictionary<MemberInfo, UniversalConfig> MemberInfoConfigs { get; }
    Dictionary<Type, UniversalConfig> TypeConfigs { get; }
    HashSet<Type> ExcludedTypes { get; }
    HashSet<MemberInfo> ExcludedMembers { get; }
    ITypeConfigurator<TOwner, T> ConfigureType<T>();
}