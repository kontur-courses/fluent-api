using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private readonly HashSet<Type> excludedTypes = new();
    private readonly Dictionary<Type, Func<object, string>> customTypeSerialization = new();
    private readonly Dictionary<Type, CultureInfo> specifiedCultureInfo = new();
    protected readonly Dictionary<string, Func<object, string>> CustomMemberSerialization = new();
    protected readonly Dictionary<string, int> MemberMaxStringLength = new();
    protected readonly HashSet<string> ExcludedMembers = new();

    private readonly Type[] finalTypes =
    {
        typeof(int), typeof(double), typeof(float), typeof(string),
        typeof(DateTime), typeof(TimeSpan), typeof(Guid), typeof(uint),
        typeof(decimal), typeof(long), typeof(ushort), typeof(ulong), typeof(short)
    };

    public PrintingConfig()
    {
    }

    protected PrintingConfig(PrintingConfig<TOwner> printingConfig)
    {
        excludedTypes = printingConfig.excludedTypes;
        customTypeSerialization = printingConfig.customTypeSerialization;
        specifiedCultureInfo = printingConfig.specifiedCultureInfo;
        CustomMemberSerialization = printingConfig.CustomMemberSerialization;
        MemberMaxStringLength = printingConfig.MemberMaxStringLength;
        ExcludedMembers = printingConfig.ExcludedMembers;
    }

    public string PrintToString(TOwner obj)
    {
        return PrintToString(obj, 0, obj.GetType().Name, new List<object>());
    }

    public PrintingConfig<TOwner> ExcludeType<T>()
    {
        excludedTypes.Add(typeof(T));
        return this;
    }

    public PrintingConfig<TOwner> WithSerializationForType<T>(Func<T, string> serializationFunc)
    {
        var type = typeof(T);
        customTypeSerialization[type] = obj => serializationFunc((T) obj);
        return this;
    }

    public PrintingConfig<TOwner> SetCultureForType<T>(CultureInfo cultureInfo) where T : IFormattable
    {
        var type = typeof(T);
        specifiedCultureInfo[type] = cultureInfo;
        return this;
    }

    public PrintingStringMemberConfig<TOwner> SelectMember(Expression<Func<TOwner, string>> member)
    {
        PrintingConfigUtils.ValidateMemberExpression(member);
        return new PrintingStringMemberConfig<TOwner>(this, member);
    }

    public PrintingMemberConfig<TOwner, T> SelectMember<T>(Expression<Func<TOwner, T>> member)
    {
        PrintingConfigUtils.ValidateMemberExpression(member);
        return new PrintingMemberConfig<TOwner, T>(this, member);
    }

    private string PrintWithCultureIfExists(object obj, string initial)
    {
        if (obj is not IFormattable formattable ||
            !specifiedCultureInfo.TryGetValue(obj.GetType(), out var cultureInfo))
            return initial;

        return formattable.ToString("", cultureInfo) + Environment.NewLine;
    }

    private string PrintToString(object obj, int nestingLevel, string memberPrefix, List<object> parents)
    {
        if (parents.Contains(obj))
            return "CYCLIC_REFERENCE" + Environment.NewLine;
        
        if (obj is null)
            return "null" + Environment.NewLine;

        if (excludedTypes.Contains(obj.GetType()) || ExcludedMembers.Contains(memberPrefix))
            return "";

        if (CustomMemberSerialization.TryGetValue(memberPrefix, out var memberSerialization))
            return memberSerialization(obj) + Environment.NewLine;

        if (customTypeSerialization.TryGetValue(obj.GetType(), out var typeSerialization))
            return typeSerialization(obj) + Environment.NewLine;

        if (MemberMaxStringLength.TryGetValue(memberPrefix, out var maxLength))
            return CutString((string) obj, maxLength) + Environment.NewLine;

        if (finalTypes.Contains(obj.GetType()))
            return PrintWithCultureIfExists(obj, obj + Environment.NewLine);

        // if (obj is IEnumerable enumerable)
        //     return PrintEnumerable(enumerable, nestingLevel, memberPrefix, parents);
       
        return PrintObjectMembers(obj, nestingLevel, memberPrefix, parents);
    }

    /*private string PrintEnumerable(IEnumerable obj, int nestingLevel, string memberPrefix, List<object> parents)
    {
        var enumerator = obj.GetEnumerator();
        using var disposable = enumerator as IDisposable;
        if (!enumerator.MoveNext())
            return "EMPTY_COLLECTION" + Environment.NewLine;
        if (excludedTypes.Contains(enumerator.Current!.GetType()))
            return "";

        var sb = new StringBuilder();
        enumerator.Reset();
        while (enumerator.MoveNext())
        {
            
        }
    }*/

    private static string CutString(string s, int maxLength)
    {
        return s.Length > maxLength ? s[..maxLength] : s;
    }

    private string PrintObjectMembers(object obj, int nestingLevel, string memberPrefix, List<object> parents)
    {
        var indentation = new string('\t', nestingLevel + 1);
        var objectType = obj.GetType();
        var sb = new StringBuilder().AppendLine(objectType.Name);

        if (memberPrefix != "")
            memberPrefix += ".";

        parents.Add(obj);
        foreach (var propertyInfo in objectType.GetProperties())
        {
            sb.Append(ProcessMember(propertyInfo.PropertyType, propertyInfo.Name, propertyInfo.GetValue(obj),
                indentation,
                memberPrefix, nestingLevel, parents));
        }

        foreach (var fieldInfo in objectType.GetFields())
        {
            sb.Append(ProcessMember(fieldInfo.FieldType, fieldInfo.Name, fieldInfo.GetValue(obj), indentation,
                memberPrefix,
                nestingLevel, parents));
        }

        parents.Remove(parents.Count - 1);
        return sb.ToString();
    }

    private string ProcessMember(Type type, string name, object value, string indentation, string memberPrefix,
        int nestingLevel, List<object> parents)
    {
        var sb = new StringBuilder();

        if (excludedTypes.Contains(type) || ExcludedMembers.Contains(memberPrefix + name))
            return "";

        sb.Append(indentation + name + " = ");
            
        sb.Append(CustomMemberSerialization.TryGetValue(memberPrefix + name, out var serialization)
            ? serialization(value) + Environment.NewLine
            : PrintToString(value, nestingLevel + 1, memberPrefix + name, parents));

        return sb.ToString();
    }
}