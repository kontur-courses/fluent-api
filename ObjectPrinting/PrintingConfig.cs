using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

public partial class PrintingConfig<TOwner>
{
    private static readonly HashSet<Type> finalTypes = new()
    {
        typeof(int),
        typeof(double),
        typeof(float),
        typeof(string),
        typeof(DateTime),
        typeof(TimeSpan)
    };

    private readonly HashSet<Type> excludedTypes = new();
    private readonly HashSet<MemberInfo> excludedMembers = new();
    private readonly Dictionary<Type, Func<object, string>> typeSpecificSerializers = new();
    private readonly Dictionary<Type, CultureInfo> typeCultureSettings = new();
    private readonly Dictionary<MemberInfo, Func<object, string>> memberSpecificSerializers = new();
    private readonly Dictionary<MemberInfo, CultureInfo> memberCultureSettings = new();
    private readonly Dictionary<MemberInfo, int> memberTrimLengths = new();
    private int stringTrimLength = -1;

    private readonly Dictionary<object, Guid> alreadyPrinted = new();

    public string PrintToString(TOwner? obj)
    {
        return PrintToString(obj, 0);
    }

    public PrintingConfig<TOwner> ForProperties<TProperty>(Action<BasicTypeConfig<TProperty>> options)
    {
        options(new BasicTypeConfig<TProperty>(this));
        return this;
    }

    public PrintingConfig<TOwner> ForProperties<TProperty>(Action<FormattableTypeConfig<TProperty>> options)
        where TProperty : IFormattable
    {
        options(new FormattableTypeConfig<TProperty>(this));
        return this;
    }

    public PrintingConfig<TOwner> ForProperty<TProperty>(Expression<Func<TOwner, TProperty>> propertyAccess, Action<BasicMemberConfig<TProperty>> options)
    {
        return ForProperty(GetMemberInfo(propertyAccess), options);
    }

    public PrintingConfig<TOwner> ForProperty<TProperty>(MemberInfo member, Action<BasicMemberConfig<TProperty>> options)
    {
        options(new BasicMemberConfig<TProperty>(this, member));
        return this;
    }

    public PrintingConfig<TOwner> ForProperty<TProperty>(Expression<Func<TOwner, TProperty>> propertyAccess, Action<FormattableMemberConfig<TProperty>> options)
        where TProperty : IFormattable
    {
        return ForProperty(GetMemberInfo(propertyAccess), options);
    }

    public PrintingConfig<TOwner> ForProperty<TProperty>(MemberInfo member, Action<FormattableMemberConfig<TProperty>> options)
        where TProperty : IFormattable
    {
        options(new FormattableMemberConfig<TProperty>(this, member));
        return this;
    }

    public PrintingConfig<TOwner> ForProperty(Expression<Func<TOwner, string>> propertyAccess, Action<StringMemberConfig> options)
    {
        return ForProperty(GetMemberInfo(propertyAccess), options);
    }

    public PrintingConfig<TOwner> ForProperty(MemberInfo member, Action<StringMemberConfig> options)
    {
        options(new StringMemberConfig(this, member));
        return this;
    }

    public PrintingConfig<TOwner> WithTrimLength(int trimLength)
    {
        stringTrimLength = trimLength;
        return this;
    }

    public PrintingConfig<TOwner> WithTrimLength(Expression<Func<TOwner, string>> propertyAccess, int trimLength)
    {
        return WithTrimLength(GetMemberInfo(propertyAccess), trimLength);
    }

    protected PrintingConfig<TOwner> WithTrimLength(MemberInfo member, int trimLength)
    {
        memberTrimLengths[member] = trimLength;
        return this;
    }

    public PrintingConfig<TOwner> WithCulture<T>(CultureInfo cultureInfo)
        where T : IFormattable
    {
        typeCultureSettings[typeof(T)] = cultureInfo;
        return this;
    }

    public PrintingConfig<TOwner> WithCulture<T>(Expression<Func<TOwner, T>> propertyAccess, CultureInfo cultureInfo)
        where T : IFormattable
    {
        return WithCulture<T>(GetMemberInfo(propertyAccess), cultureInfo);
    }

    protected PrintingConfig<TOwner> WithCulture<T>(MemberInfo member, CultureInfo cultureInfo)
        where T : IFormattable
    {
        memberCultureSettings[member] = cultureInfo;
        return this;
    }

    public PrintingConfig<TOwner> WithSerializer<T>(Func<T, string> serializer)
    {
        typeSpecificSerializers[typeof(T)] = x => serializer((T)x);
        return this;
    }

    public PrintingConfig<TOwner> WithSerializer<T>(Expression<Func<TOwner, T>> propertyAccess, Func<T, string> serializer)
    {
        return WithSerializer(GetMemberInfo(propertyAccess), serializer);
    }

    protected PrintingConfig<TOwner> WithSerializer<T>(MemberInfo member, Func<T, string> serializer)
    {
        memberSpecificSerializers[member] = x => serializer((T)x);
        return this;
    }

    public PrintingConfig<TOwner> Exclude<T>()
    {
        excludedTypes.Add(typeof(T));
        return this;
    }

    public PrintingConfig<TOwner> Exclude(Expression<Func<TOwner, object>> propertyAccess)
    {
        return Exclude(GetMemberInfo(propertyAccess));
    }

    protected PrintingConfig<TOwner> Exclude(MemberInfo member)
    {
        excludedMembers.Add(member);
        return this;
    }

    private static MemberInfo GetMemberInfo<TObject, TMember>(Expression<Func<TObject, TMember>> memberAccess)
    {
        if (memberAccess.Body.NodeType != ExpressionType.MemberAccess)
            throw new ArgumentException("Expression should represent member access", nameof(memberAccess));
        return ((MemberExpression)memberAccess.Body).Member;
    }

    private string PrintToString(object? obj, int nestingLevel, MemberInfo? currentMember = null)
    {
        if (obj == null)
            return "null";

        if (alreadyPrinted.TryGetValue(obj, out var id))
            return $"was printed before : {id}";

        if (TrySerializeWithMemberInfo(obj!, currentMember, out var serialized))
            return serialized;

        var type = obj.GetType();

        if (TrySerializeCollection(obj!, nestingLevel, out serialized))
            return serialized;

        if (TrySerializeByType(obj!, type, out serialized))
            return serialized;

        return SerializeProperties(obj!, type, nestingLevel);
    }

    private string SerializeProperties(object obj, Type type, int nestingLevel)
    {
        var id = Guid.NewGuid();
        alreadyPrinted.Add(obj, id);
        var identation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        sb.Append($"{type.Name} - {id}");
        foreach (var propertyInfo in type.GetProperties())
        {
            if (excludedMembers.Contains(propertyInfo))
                continue;

            if (excludedTypes.Contains(propertyInfo.PropertyType))
                continue;

            var serialized = PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1, propertyInfo);
            sb.Append($"{Environment.NewLine}{identation}{propertyInfo.Name} = {serialized}");
        }

        return sb.ToString();
    }

    private bool TrySerializeCollection(object collection, int nestingLevel, out string serialized)
    {
        serialized = string.Empty;
        if (collection is not ICollection)
            return false;

        var identation = new string('\t', nestingLevel + 1);
        if (collection is IDictionary dictionary)
        {
            serialized = SerializeDictionary(dictionary, nestingLevel, identation);
            return true;
        }

        serialized = SerializeCollection((collection as ICollection)!, nestingLevel, identation);
        return true;
    }

    private string SerializeCollection(ICollection collection, int nestingLevel, string identation)
    {
        var sb = new StringBuilder($"[{Environment.NewLine}");
        foreach (var obj in collection)
        {
            sb.Append($"{identation}{PrintToString(obj, nestingLevel + 1)},{Environment.NewLine}");
        }

        sb.Remove(sb.Length - 2, 2);
        sb.Append($"{Environment.NewLine}{identation}]");

        return sb.ToString();
    }

    private string SerializeDictionary(IDictionary dictionary, int nestingLevel, string identation)
    {
        var sb = new StringBuilder($"{{{Environment.NewLine}");
        foreach (DictionaryEntry obj in dictionary)
        {
            sb.Append($"{identation}{PrintToString(obj.Key, nestingLevel + 1)} : {PrintToString(obj.Value, nestingLevel + 1)},{Environment.NewLine}");
        }

        sb.Append($"{Environment.NewLine}{identation}}}");
        return sb.ToString();

    }

    private bool TrySerializeByType(object obj, Type type, out string serialized)
    {
        serialized = string.Empty;
        if (typeSpecificSerializers.TryGetValue(type, out var serializer))
        {
            serialized = serializer(obj);
            return true;
        }

        if (obj is string)
        {
            serialized = ToTrimmedString((obj as string)!, stringTrimLength);
            return true;
        }

        if (finalTypes.Contains(type))
        {
            serialized = typeCultureSettings.TryGetValue(type, out var culture)
                ? ToStringAsFormattable(obj, culture)
                : obj.ToString()!;
            return true;
        }

        return false;
    }

    private bool TrySerializeWithMemberInfo(object? obj, MemberInfo? memberInfo, out string serialized)
    {
        serialized = string.Empty;

        if (memberInfo == null)
            return false;

        if (memberSpecificSerializers.TryGetValue(memberInfo, out var serializer))
        {
            serialized = serializer(obj!);
            return true;
        }

        if (memberCultureSettings.TryGetValue(memberInfo, out var culture))
        {
            serialized = ToStringAsFormattable(obj!, culture);
            return true;
        }

        if (memberTrimLengths.TryGetValue(memberInfo, out var trimLength))
        {
            serialized = ToTrimmedString((obj as string)!, trimLength);
            return true;
        }

        return false;
    }

    private static string ToTrimmedString(string str, int trimLength)
    {
        if (trimLength == -1 || str.Length <= trimLength)
            return str;
        if (trimLength < 0)
            throw new InvalidOperationException($"TrimLength, set for type or member, is invalid: {trimLength}");
        return $"{str[..trimLength]}...";
    }

    private static string ToStringAsFormattable(object obj, CultureInfo culture)
    {
        return (obj as IFormattable)!.ToString(null, culture);
    }
}
