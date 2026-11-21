using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

internal class ObjectSerializer
{
    private const string CycleMark = "<cycle>";

    private static readonly HashSet<Type> FinalTypes =
    [
        typeof(bool),
        typeof(char),
        typeof(byte),
        typeof(sbyte),
        typeof(short),
        typeof(ushort),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
        typeof(nint),
        typeof(nuint),
        typeof(Int128),
        typeof(UInt128),
        typeof(Half),
        typeof(float),
        typeof(double),
        typeof(decimal),
        typeof(string),
        typeof(Guid),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(DateOnly),
        typeof(TimeOnly),
        typeof(TimeSpan)
    ];

    private readonly PrintingSettings settings;
    private readonly HashSet<object> objectsBeingPrinted = new(ReferenceEqualityComparer.Instance);

    private ObjectSerializer(PrintingSettings settings)
    {
        this.settings = settings;
    }

    public static string Serialize(object obj, PrintingSettings settings)
    {
        var serializer = new ObjectSerializer(settings);
        return serializer.PrintToString(obj, 0);
    }

    private string PrintToString(object obj, int nestingLevel)
    {
        if (obj == null)
            return "null";

        var type = obj.GetType();
        if (settings.TryGetTypePrinter(type, out var typePrinter))
            return typePrinter(obj);

        if (IsFinalType(type))
            return obj.ToString();

        if (objectsBeingPrinted.Contains(obj))
            return CycleMark;

        objectsBeingPrinted.Add(obj);
        var printed = PrintComposite(obj, type, nestingLevel);
        objectsBeingPrinted.Remove(obj);
        return printed;
    }

    private string PrintComposite(object obj, Type type, int nestingLevel)
    {
        var indentation = new string('\t', nestingLevel + 1);
        var builder = new StringBuilder(type.Name);
        foreach (var line in GetLines(obj, type, nestingLevel))
        {
            builder.Append(Environment.NewLine + indentation + line);
        }

        return builder.ToString();
    }

    private IEnumerable<string> GetLines(object obj, Type type, int nestingLevel)
    {
        return obj switch
        {
            IDictionary dictionary => GetDictionaryLines(dictionary, nestingLevel),
            IEnumerable collection => GetCollectionLines(collection, nestingLevel),
            _ => GetMemberLines(obj, type, nestingLevel)
        };
    }

    private IEnumerable<string> GetDictionaryLines(IDictionary dictionary, int nestingLevel)
    {
        foreach (DictionaryEntry entry in dictionary)
        {
            var printedKey = PrintToString(entry.Key, nestingLevel + 1);
            var printedValue = PrintToString(entry.Value, nestingLevel + 1);
            yield return printedKey + " = " + printedValue;
        }
    }

    private IEnumerable<string> GetCollectionLines(IEnumerable collection, int nestingLevel)
    {
        foreach (var element in collection)
        {
            yield return PrintToString(element, nestingLevel + 1);
        }
    }

    private IEnumerable<string> GetMemberLines(object obj, Type type, int nestingLevel)
    {
        foreach (var member in GetPrintedMembers(type))
        {
            if (settings.IsExcluded(member, GetMemberType(member)))
                continue;

            var printedValue = PrintMember(member, GetMemberValue(member, obj), nestingLevel);
            yield return member.Name + " = " + printedValue;
        }
    }

    private string PrintMember(MemberInfo member, object value, int nestingLevel)
    {
        return settings.TryGetMemberPrinter(member, out var memberPrinter)
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
        return type.IsEnum
            || FinalTypes.Contains(type);
    }
}