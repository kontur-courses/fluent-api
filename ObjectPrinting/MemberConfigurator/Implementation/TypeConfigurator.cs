using System;
using System.Collections.Generic;
using System.Globalization;
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

    public IObjectConfiguration<TOwner> Configure(CultureInfo cultureInfo)
    {
        if (!objectConfiguration.TypeConfigs.ContainsKey(typeof(T)))
            objectConfiguration.TypeConfigs.Add(typeof(T), new List<Func<string, string>>());
        
        objectConfiguration.TypeConfigs[typeof(T)].Add(s => s.ToString(cultureInfo));
        return objectConfiguration;
    }


    public IObjectConfiguration<TOwner> Configure(int length)
    {
        if (!objectConfiguration.TypeConfigs.ContainsKey(typeof(T)))
            objectConfiguration.TypeConfigs.Add(typeof(T), new List<Func<string, string>>());

        objectConfiguration.TypeConfigs[typeof(T)].Add(s => s[..length]);
        return objectConfiguration;
    }
}