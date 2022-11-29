using System;
using System.Collections.Generic;
using System.Globalization;

namespace ObjectPrinting;

public class TypeConfigurator<TOwner, T> : ITypeConfigurator<TOwner, T>
{
    private readonly ObjectConfigurator<TOwner> objectConfigurator;

    public TypeConfigurator(ObjectConfigurator<TOwner> objectConfigurator)
    {
        this.objectConfigurator = objectConfigurator;
    }


    public IBasicConfigurator<TOwner> Configure(CultureInfo cultureInfo)
    {
        if (!objectConfigurator.TypeConfigs.ContainsKey(typeof(T)))
            objectConfigurator.TypeConfigs.Add(typeof(T), new UniversalConfig());
        
        objectConfigurator.TypeConfigs[typeof(T)].CultureInfo = cultureInfo;
        return objectConfigurator;
    }


    public IBasicConfigurator<TOwner> Configure(int length)
    {
        if (!objectConfigurator.TypeConfigs.ContainsKey(typeof(T)))
            objectConfigurator.TypeConfigs.Add(typeof(T), new UniversalConfig());

        objectConfigurator.TypeConfigs[typeof(T)].TrimLength = length;
        return objectConfigurator;
    }
}