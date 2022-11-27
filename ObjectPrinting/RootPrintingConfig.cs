using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

public class RootPrintingConfig<TOwner> : PrintingConfig<TOwner>, IInternalPrintingConfigStorage
{
    internal RootPrintingConfig() : base(null)
    {
    }

    public LinkedList<(Type Type, Func<object, string> Serializer)>
        AssignableTypeSerializers { get; } = new(
        new (Type Type, Func<object, string> Serializer)[]
        {
            (typeof(string), o => o.ToString() ?? string.Empty),
            (typeof(IFormattable), o => ((IFormattable)o).ToString(null, CultureInfo.CurrentCulture))
        });

    public HashSet<Type> IgnoredTypesFromAssignableCheck { get; } = new();
    public HashSet<MemberInfo> MemberExcluding { get; } = new();

    public Dictionary<MemberInfo, Func<object, string>> MemberSerializers { get; } = new();

    public HashSet<Type> TypeExcluding { get; } = new();

    public Dictionary<Type, Func<object, string>> TypeSerializers { get; } = new();

    public void PrintToString(object obj, int nestingLevel, StringBuilder stringBuilder)
    {
        var indentation = new string('\t', nestingLevel + 1);
        var type = obj.GetType();
        stringBuilder.AppendLine(type.Name);
        var iinternalPrintingConfig = this as IInternalPrintingConfig<TOwner>;
        var root = iinternalPrintingConfig.GetRoot();
        foreach (var propertyInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            root.ConsumeProperty(obj, nestingLevel, stringBuilder, propertyInfo, indentation);

        foreach (var fieldInfo in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            root.ConsumeField(obj, nestingLevel, stringBuilder, fieldInfo, indentation);
    }
}