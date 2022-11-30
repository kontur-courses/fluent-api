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
            objectConfiguration.TypeConfigs.Add(typeof(T), new UniversalConfig());
        
        objectConfiguration.TypeConfigs[typeof(T)].CultureInfo = cultureInfo;
        return objectConfiguration;
    }


    public IObjectConfiguration<TOwner> Configure(int length)
    {
        if (!objectConfiguration.TypeConfigs.ContainsKey(typeof(T)))
            objectConfiguration.TypeConfigs.Add(typeof(T), new UniversalConfig());

        objectConfiguration.TypeConfigs[typeof(T)].TrimLength = length;
        return objectConfiguration;
    }
}