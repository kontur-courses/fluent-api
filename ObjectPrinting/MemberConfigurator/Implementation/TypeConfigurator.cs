using System;
using ObjectPrinting.ObjectConfiguration;
using ObjectPrinting.ObjectConfiguration.Implementation;

namespace ObjectPrinting.MemberConfigurator.Implementation;

public class TypeConfigurator<TOwner, T> : IMemberConfigurator<TOwner, T>
{
    private readonly ObjectConfiguration<TOwner> objectConfiguration;

    public TypeConfigurator(ObjectConfiguration<TOwner> objectConfiguration)
    {
        this.objectConfiguration = objectConfiguration;
    }

    public IObjectConfiguration<TOwner> Configure(Func<T, string> func)
    {
        if (!objectConfiguration.TypeConfigs.ContainsKey(typeof(T)))
            objectConfiguration.TypeConfigs.Add(typeof(T), i => func((T) i));
        
        return objectConfiguration;
    }
}