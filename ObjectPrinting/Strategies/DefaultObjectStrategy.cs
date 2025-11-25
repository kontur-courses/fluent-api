using System;
using System.Reflection;
using System.Text;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Strategies;

public class DefaultObjectStrategy : IPrintStrategy
{
    public bool CanPrint<TOwner>(object obj, Type type, PrintingConfig<TOwner> config)
        => true; 

    public void Print<TOwner>(object obj, Type type,
        PrintingConfig<TOwner> config, StringBuilder sb, int indent)
    {
        sb.AppendLine(type.Name + ":");

        foreach (var prop in type.GetProperties())
        {
            if (ShouldSkipProperty(prop, config))
                continue;

            PrintProperty(obj, prop, config, sb, indent);
        }
    }
    
    #region Helpers
    
    private static bool ShouldSkipProperty<TOwner>(
        PropertyInfo prop,
        PrintingConfig<TOwner> config)
    {
        var name = prop.Name;
        var type = prop.PropertyType;

        return config.ExcludedProperties.Contains(name)
            || config.ExcludedTypes.Contains(type);
    }

    private static void PrintProperty<TOwner>(object obj, PropertyInfo prop,
        PrintingConfig<TOwner> config, StringBuilder sb, int indent)
    {
        var name = prop.Name;
        var value = prop.GetValue(obj);

        PrintPropertyHeader(sb, indent, name);

        if (TryPrintWithCustomSerializer(name, value, config, sb)) return;
        if (TryPrintStringTrimmed(name, value, config, sb)) return;
        if (TryPrintNull(value, sb)) return;

        PrintNestedObject(value, config, sb, indent + 1);
    }

    private static void PrintPropertyHeader(StringBuilder sb, int indent, string name)
    {
        sb.Append(new string('\t', indent + 1));
        sb.Append(name);
        sb.Append(" = ");
    }

    private static bool TryPrintWithCustomSerializer<TOwner>(
        string name,
        object? value,
        PrintingConfig<TOwner> config,
        StringBuilder sb)
    {
        if (!config.PropertySerializers.TryGetValue(name, out var serializer))
            return false;

        sb.AppendLine(value != null ? serializer(value) : "null");
        return true;
    }

    private static bool TryPrintStringTrimmed<TOwner>(string name, object? value,
        PrintingConfig<TOwner> config, StringBuilder sb)
    {
        if (value is not string s ||
            !config.StringTrimmingRules.TryGetValue(name, out var max))
            return false;

        sb.AppendLine(s.Length <= max ? s : s[..max]);
        return true;
    }

    private static bool TryPrintNull(object? value, StringBuilder sb)
    {
        if (value is not null)
            return false;

        sb.AppendLine("null");
        return true;
    }

    private static void PrintNestedObject<TOwner>(
        object? value,
        PrintingConfig<TOwner> config,
        StringBuilder sb,
        int indent)
    {
        value.Print(config, sb, indent);
        sb.AppendLine();
    }
    
    #endregion
}
