using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

internal class ObjectSerializer
{
    private static readonly Type[] FinalTypes =
    [
        typeof(string),
        typeof(decimal),
        typeof(Guid),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(TimeSpan)
    ];

    private readonly IPrintingSettings _settings;

    private ObjectSerializer(IPrintingSettings settings)
    {
        _settings = settings;
    }

    public static string Serialize(object obj, IPrintingSettings settings)
    {
        var serializer = new ObjectSerializer(settings);
        return serializer.PrintToString(obj, 0);
    }

    private string PrintToString(object obj, int nestingLevel)
    {
        if (obj == null)
            return "null";

        var type = obj.GetType();
        if (_settings.TryGetTypePrinter(type, out var typePrinter))
            return typePrinter(obj);

        if (IsFinalType(type))
            return obj.ToString();

        return PrintMembers(obj, type, nestingLevel);
    }

    private string PrintMembers(object obj, Type type, int nestingLevel)
    {
        var indentation = new string('\t', nestingLevel + 1);
        var builder = new StringBuilder(type.Name);
        foreach (var member in GetPrintedMembers(type))
        {
            if (_settings.IsExcluded(member, GetMemberType(member)))
                continue;

            var printedValue = PrintMember(member, GetMemberValue(member, obj), nestingLevel);
            builder.Append(Environment.NewLine + indentation + member.Name + " = " + printedValue);
        }

        return builder.ToString();
    }

    private string PrintMember(MemberInfo member, object value, int nestingLevel)
    {
        return _settings.TryGetMemberPrinter(member, out var memberPrinter)
            ? memberPrinter(value)
            : PrintToString(value, nestingLevel + 1);
    }

    private static IEnumerable<MemberInfo> GetPrintedMembers(Type type)
    {
        return type
            .GetProperties()
            .Concat<MemberInfo>(type.GetFields());
    }

    private static Type GetMemberType(MemberInfo member)
    {
        return member is PropertyInfo property
            ? property.PropertyType
            : ((FieldInfo)member).FieldType;
    }

    private static object GetMemberValue(MemberInfo member, object obj)
    {
        return member is PropertyInfo property
            ? property.GetValue(obj)
            : ((FieldInfo)member).GetValue(obj);
    }

    private static bool IsFinalType(Type type)
    {
        return type.IsPrimitive
            || type.IsEnum
            || FinalTypes.Contains(type);
    }
}