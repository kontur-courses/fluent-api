using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
{
    private readonly HashSet<Type> _excludedTypes = [];
    private readonly HashSet<MemberInfo> _excludedMembers = [];
    private readonly Dictionary<Type, Func<object, string>> _typeSerializers = [];
    private readonly Dictionary<MemberInfo, Func<object, string>> _memberSerializers = [];
    private readonly Dictionary<Type, CultureInfo> _typeCultures = [];
    private readonly Dictionary<MemberInfo, CultureInfo> _memberCultures = [];
    private readonly Dictionary<MemberInfo, int> _memberStringTrims = [];
    private readonly Dictionary<Type, int> _typeStringTrims = [];

    private readonly object _sync = new();

    internal void SetTypeSerializer(Type type, Func<object, string> serializer)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(serializer);

        var normalizedType = NormalizeType(type);

        lock (_sync)
        {
            _typeSerializers[normalizedType] = serializer;
        }
    }

    internal void SetMemberSerializer(MemberInfo member, Func<object, string> serializer)
    {
        ArgumentNullException.ThrowIfNull(member);
        ArgumentNullException.ThrowIfNull(serializer);

        lock (_sync)
        {
            _memberSerializers[member] = serializer;
        }
    }

    internal void SetTypeCulture(Type type, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(culture);

        var normalizedType = NormalizeType(type);

        lock (_sync)
        {
            _typeCultures[normalizedType] = culture;
        }
    }

    internal void SetMemberCulture(MemberInfo member, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(member);
        ArgumentNullException.ThrowIfNull(culture);

        lock (_sync)
        {
            _memberCultures[member] = culture;
        }
    }

    internal void SetMemberStringTrim(MemberInfo member, int maxLength)
    {
        ArgumentNullException.ThrowIfNull(member);
        if (maxLength < 0)
            throw new ArgumentOutOfRangeException(nameof(maxLength), "Параметр maxLength должен быть неотрицательным.");

        lock (_sync)
        {
            _memberStringTrims[member] = maxLength;
        }
    }

    internal void SetTypeStringTrim(Type type, int maxLength)
    {
        ArgumentNullException.ThrowIfNull(type);
        if (maxLength < 0)
            throw new ArgumentOutOfRangeException(nameof(maxLength), "Параметр maxLength должен быть неотрицательным.");

        var normalizedType = NormalizeType(type);

        lock (_sync)
        {
            _typeStringTrims[normalizedType] = maxLength;
        }
    }

    public IPrintingConfig<TOwner> Excluding<TPropType>()
    {
        var normalizedType = NormalizeType(typeof(TPropType));

        lock (_sync)
        {
            _excludedTypes.Add(normalizedType);
            return this;
        }
    }

    public IPrintingConfig<TOwner> Excluding<TProp>(Expression<Func<TOwner, TProp>> memberSelector)
    {
        ArgumentNullException.ThrowIfNull(memberSelector);

        if (memberSelector.Body is not MemberExpression expression)
            throw new ArgumentException("Выражение должно указывать на поле или свойство.", nameof(memberSelector));

        lock (_sync)
        {
            _excludedMembers.Add(expression.Member);
            return this;
        }
    }

    public IPropertyPrintingConfig<TOwner, TProp> Printing<TProp>()
    {
        var normalizedType = NormalizeType(typeof(TProp));
        return new PropertyPrintingConfig<TOwner, TProp>(this, normalizedType, null!);
    }

    public IPropertyPrintingConfig<TOwner, TProp> Printing<TProp>(Expression<Func<TOwner, TProp>> memberSelector)
    {
        ArgumentNullException.ThrowIfNull(memberSelector);

        if (memberSelector.Body is not MemberExpression expression)
            throw new ArgumentException("Выражение должно указывать на поле или свойство.", nameof(memberSelector));

        var normalizedType = NormalizeType(typeof(TProp));
        return new PropertyPrintingConfig<TOwner, TProp>(this, normalizedType, expression.Member);
    }

    public string PrintToString(TOwner obj)
    {
        lock (_sync)
        {
            var visited = new HashSet<object>(new ReferenceEqualityComparer());
            var sb = new StringBuilder();

            AppendTo(sb, obj!, 0, visited, null!, typeof(TOwner));

            return sb.ToString();
        }
    }

    private void AppendTo(StringBuilder sb, object obj, int nestingLevel, HashSet<object> visited, MemberInfo member, Type expectedType)
    {
        if (obj == null)
        {
            sb.Append("null (");
            sb.Append(expectedType.Name);
            sb.Append(')');
            sb.Append(Environment.NewLine);
            return;
        }

        var type = NormalizeType(obj.GetType());

        if (member != null && _memberSerializers.TryGetValue(member, out var memberSerializer))
        {
            sb.Append(memberSerializer(obj));
            sb.Append(Environment.NewLine);
            return;
        }

        if (_typeSerializers.TryGetValue(type, out var typeSerializer))
        {
            sb.Append(typeSerializer(obj));
            sb.Append(Environment.NewLine);
            return;
        }

        if (IsFinalType(type))
        {
            sb.Append(FormatFinalValue(obj, type, member));
            sb.Append(Environment.NewLine);
            return;
        }

        if (!visited.Add(obj))
        {
            sb.Append("cyclic reference");
            sb.Append(Environment.NewLine);
            return;
        }

        try
        {
            sb.Append(obj.GetType().Name);
            sb.Append(Environment.NewLine);

            var indent = new string('\t', nestingLevel + 1);

            foreach (var memberInfo in GetSerializableMembers(obj.GetType()))
            {
                if (ShouldSkipMember(memberInfo))
                    continue;

                var value = GetMemberValue(obj, memberInfo);
                var memberType = GetMemberType(memberInfo);

                sb.Append(indent);
                sb.Append(memberInfo.Name);
                sb.Append(" = ");

                AppendTo(sb, value!, nestingLevel + 1, visited, memberInfo, memberType);
            }
        }
        finally
        {
            visited.Remove(obj);
        }
    }

    private static Type GetMemberType(MemberInfo member)
    {
        return member switch
        {
            FieldInfo f => f.FieldType,
            PropertyInfo p => p.PropertyType,
            _ => typeof(object)
        };
    }

    private static IEnumerable<MemberInfo> GetSerializableMembers(Type type)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

        foreach (var member in type.GetFields(flags))
            yield return member;

        foreach (var member in type.GetProperties(flags))
        {
            var getter = member.GetMethod;
            if (getter is null || !getter.IsPublic)
                continue;

            yield return member;
        }
    }

    private static object? GetMemberValue(object obj, MemberInfo member)
    {
        try
        {
            return member switch
            {
                FieldInfo field => field.GetValue(obj),
                PropertyInfo property => property.GetValue(obj),
                _ => null
            };
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            return $"<exception: {ex.InnerException.GetType().Name}>";
        }
        catch (Exception ex)
        {
            return $"<exception: {ex.GetType().Name}>";
        }
    }

    private static bool IsFinalType(Type type)
    {
        type = NormalizeType(type);

        return type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(TimeSpan)
            || type == typeof(Guid);
    }

    private string FormatFinalValue(object obj, Type type, MemberInfo? member)
    {
        type = NormalizeType(type);

        string result;

        CultureInfo? culture = null;
        if (member != null && _memberCultures.TryGetValue(member, out var memberCulture))
            culture = memberCulture;
        else if (_typeCultures.TryGetValue(type, out var typeCulture))
            culture = typeCulture;

        if (obj is IFormattable formattable && culture != null)
            result = formattable.ToString(null, culture) ?? string.Empty;
        else
            result = obj.ToString() ?? string.Empty;

        if (type == typeof(string))
        {
            var needsTrim = false;
            var maxLength = 0;

            if (member != null && _memberStringTrims.TryGetValue(member, out var memberMax))
            {
                needsTrim = result.Length > memberMax;
                maxLength = memberMax;
            }
            else if (_typeStringTrims.TryGetValue(type, out var typeMax))
            {
                needsTrim = result.Length > typeMax;
                maxLength = typeMax;
            }

            if (needsTrim)
                result = result[..maxLength];
        }

        return result;
    }

    private bool ShouldSkipMember(MemberInfo member)
    {
        if (_excludedMembers.Contains(member))
            return true;

        var type = member switch
        {
            PropertyInfo p => p.PropertyType,
            FieldInfo f => f.FieldType,
            _ => null
        };

        return type != null && _excludedTypes.Contains(NormalizeType(type));
    }

    private static Type NormalizeType(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying != null)
            return underlying;

        return type;
    }
}
