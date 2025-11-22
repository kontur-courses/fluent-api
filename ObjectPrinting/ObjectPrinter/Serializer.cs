using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.ObjectPrinter;

internal static class Serializer
{
    private static readonly HashSet<Type> TerminalTypes =
    [
        typeof(string), typeof(DateTime), typeof(TimeSpan),
        typeof(Guid), typeof(decimal),
        typeof(byte), typeof(sbyte), typeof(short), typeof(ushort),
        typeof(int), typeof(uint), typeof(long), typeof(ulong),
        typeof(float), typeof(double), typeof(bool), typeof(char)
    ];

    internal static string SerializeObject<TOwner>(TOwner? obj, PrintingConfig<TOwner> config)
    {
        var sb = new StringBuilder();
        var ctx = new SerializationContext<TOwner>(config, sb);
        Serialize(obj, 0, null, ctx);
        return sb.ToString();
    }

    private static void Serialize<TOwner>(
        object? obj,
        int level,
        MemberInfo? parentMember,
        SerializationContext<TOwner> ctx)
    {
        if (TryHandleNullOrExcludedOrDepth(obj, level, ctx))
            return;

        var type = obj!.GetType();

        if (TryHandleCycle(obj, type, ctx))
            return;

        if (TryHandleCustomTypeSerializer(obj, type, parentMember, ctx))
            return;

        if (TryHandleTerminal(obj, type, parentMember, ctx))
            return;

        if (TryHandleDictionary(obj, type, level, ctx))
            return;

        if (TryHandleEnumerable(obj, type, level, ctx))
            return;

        SerializeComplexObject(obj, type, level, ctx);
    }


    private static bool TryHandleNullOrExcludedOrDepth<TOwner>(
        object? obj,
        int level,
        SerializationContext<TOwner> ctx)
    {
        if (obj is null)
        {
            ctx.Builder.AppendLine("null");
            return true;
        }

        var type = obj.GetType();

        if (ctx.Config.ExcludedTypes.Contains(type))
        {
            ctx.Builder.AppendLine("[Excluded Type]");
            return true;
        }

        if (level >= ctx.Config.MaxNestingLevel)
        {
            ctx.Builder.AppendLine($"[{FormatTypeName(type)} превышен уровень вложенности]");
            return true;
        }

        return false;
    }

    private static bool TryHandleCycle<TOwner>(
        object obj,
        Type type,
        SerializationContext<TOwner> ctx)
    {
        if (type.IsValueType)
            return false;

        if (ctx.Visited.Add(obj)) 
            return false;
        ctx.Builder.AppendLine($"[CyclicRef {FormatTypeName(type)}]");
        return true;

    }

    private static bool TryHandleCustomTypeSerializer<TOwner>(
        object obj,
        Type type,
        MemberInfo? parentMember,
        SerializationContext<TOwner> ctx)
    {
        if (!ctx.Config.CustomTypeSerializers.TryGetValue(type, out var typeSer))
            return false;

        var text = typeSer(obj);
        ctx.Builder.Append(ApplyTrimming(text, parentMember, ctx.Config));
        return true;
    }

    private static bool TryHandleTerminal<TOwner>(
        object obj,
        Type type,
        MemberInfo? parentMember,
        SerializationContext<TOwner> ctx)
    {
        if (!TerminalTypes.Contains(type) && !type.IsEnum)
            return false;

        ctx.Builder.Append(FormatScalar(obj, type, parentMember, ctx.Config));
        return true;
    }

    private static bool TryHandleDictionary<TOwner>(
        object obj,
        Type type,
        int level,
        SerializationContext<TOwner> ctx)
    {
        if (obj is not IDictionary dict)
            return false;

        ctx.Builder.AppendLine(FormatTypeName(type));
        foreach (DictionaryEntry entry in dict)
        {
            ctx.Indent(level + 1);
            ctx.Builder.Append("Key = ");
            Serialize(entry.Key, level + 1, null, ctx);

            ctx.Indent(level + 1);
            ctx.Builder.Append("Value = ");
            Serialize(entry.Value, level + 1, null, ctx);
        }

        return true;
    }

    private static bool TryHandleEnumerable<TOwner>(
        object obj,
        Type type,
        int level,
        SerializationContext<TOwner> ctx)
    {
        if (obj is not IEnumerable enumerable || type == typeof(string))
            return false;

        ctx.Builder.AppendLine(FormatTypeName(type));
        var i = 0;
        foreach (var item in enumerable)
        {
            ctx.Indent(level + 1);
            ctx.Builder.Append($"[{i}] = ");
            Serialize(item, level + 1, null, ctx);
            i++;
        }

        return true;
    }

    private static void SerializeComplexObject<TOwner>(
        object obj,
        Type type,
        int level,
        SerializationContext<TOwner> ctx)
    {
        ctx.Builder.AppendLine(FormatTypeName(type));

        foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!prop.CanRead) continue;

            SerializeMember(prop, level,
                p => p.PropertyType,
                p => p.GetValue(obj),
                p => p.Name,
                ctx);
        }

        foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            SerializeMember(field, level,
                f => f.FieldType,
                f => f.GetValue(obj),
                f => f.Name,
                ctx);
        }
    }


    private static string FormatScalar<TOwner>(
        object value,
        Type type,
        MemberInfo? parentMember,
        PrintingConfig<TOwner> config)
    {
        if (config.CustomTypeSerializers.TryGetValue(type, out var del))
        {
            var s = del(value);
            return ApplyTrimming(s, parentMember, config);
        }

        string raw;

        if (type == typeof(string))
            raw = (string)value;
        else if (config.CulturesForTypes.TryGetValue(type, out var culture) && value is IFormattable fmt)
            raw = fmt.ToString(null, culture);
        else
            raw = value.ToString() ?? string.Empty;

        return ApplyTrimming(raw, parentMember, config);
    }

    private static string ApplyTrimming<TOwner>(
        string? s,
        MemberInfo? parentMember,
        PrintingConfig<TOwner> config)
    {
        int? length = null;

        if (parentMember != null &&
            config.TrimmedMembers.TryGetValue(parentMember, out var memberLen))
        {
            length = memberLen;
        }
        else if (parentMember is PropertyInfo pi &&
                 config.TrimmedTypes.TryGetValue(pi.PropertyType, out var typeLen))
        {
            length = typeLen;
        }
        else if (parentMember is FieldInfo fi &&
                 config.TrimmedTypes.TryGetValue(fi.FieldType, out var fieldTypeLen))
        {
            length = fieldTypeLen;
        }

        if (length.HasValue && s != null && s.Length > length.Value)
            s = s[..length.Value];

        Debug.Assert(s != null, nameof(s) + " != null");
        return s.EndsWith(Environment.NewLine) ? s : s + Environment.NewLine;
    }

    private static object? SafeGet(Func<object?> getter)
    {
        try
        {
            return getter();
        }
        catch
        {
            return null;
        }
    }

    private static string FormatTypeName(Type t)
    {
        if (!t.IsGenericType)
            return t.Name;

        var defName = t.Name;
        var backtick = defName.IndexOf('`');
        defName = defName[..backtick];

        var args = t.GetGenericArguments();
        var argNames = string.Join(", ", args.Select(FormatTypeName));
        return $"{defName}<{argNames}>";
    }

    private static void SerializeMember<TOwner, TMember>(
        TMember member,
        int level,
        Func<TMember, Type> getMemberType,
        Func<TMember, object?> getValue,
        Func<TMember, string> getName,
        SerializationContext<TOwner> ctx)
        where TMember : MemberInfo
    {
        if (ctx.Config.ExcludedMember.Contains(member)) return;
        if (ctx.Config.ExcludedTypes.Contains(getMemberType(member))) return;

        ctx.Indent(level + 1);
        ctx.Builder.Append(getName(member)).Append(" = ");

        if (ctx.Config.CustomMemberSerializers.TryGetValue(member, out var memberSer))
        {
            var value = SafeGet(() => getValue(member));
            var text = value is null ? "null" : memberSer(value);
            ctx.Builder.AppendLine(ApplyTrimming(text, member, ctx.Config).TrimEnd('\r', '\n'));
            return;
        }

        var memberValue = SafeGet(() => getValue(member));
        Serialize(memberValue, level + 1, member, ctx);
    }
}