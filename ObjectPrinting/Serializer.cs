using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

public class Serializer<TOwner>
{
    private PrintingConfig<TOwner> config;

    private HashSet<object> printedObjects;

    public Serializer(PrintingConfig<TOwner> config)
    {
        this.config = config;
    }

    public string SerializeObject(TOwner obj)
    {
        printedObjects = new HashSet<object>();

        return PrintToString(obj, 0);
    }

    private string PrintToString(object obj, int nestingLevel)
    {
        if (obj == null)
            return "null" + Environment.NewLine;

        if (!obj.GetType().IsValueType && !printedObjects.Add(obj))
            return "Cycle reference detected" + Environment.NewLine;

        var indentation = GetIndentation(nestingLevel);

        if (config.CulturesForTypes.TryGetValue(obj.GetType(), out var culture))
            return ((IFormattable)obj).ToString(null, culture) + Environment.NewLine;

        if (IsTypeFinal(obj.GetType()))
        {
            var trimLen = config.TrimStringValue;
            var printObj = obj.ToString();
            if (obj is string && trimLen != null)
                printObj = printObj?.Length >= trimLen ? printObj[..(int)trimLen] : printObj;
            return printObj + Environment.NewLine;
        }

        var sb = new StringBuilder();
        var type = obj.GetType();
        sb.AppendLine(type.Name);

        if (obj is IEnumerable enumerable)
            sb.Append(GetPrintedCollection(enumerable, nestingLevel));
        else
            foreach (var memberInfo in type.GetMembers().Where(IsPropertyOrField))
            {
                if (config.ExcludedTypes.Contains(GetMemberType(memberInfo)) ||
                    config.ExcludedMembers.Contains(memberInfo))
                    continue;

                var printingResult = GetPrintingResult(obj, memberInfo, nestingLevel);

                sb.Append(indentation + memberInfo.Name + " = " + printingResult);
            }

        return sb.ToString();
    }

    private string GetPrintedCollection(IEnumerable obj, int nestingLevel)
    {
        if (obj is IDictionary dict)
            return GetPrintedDictionary(dict, nestingLevel);
        else
            return GetPrintedSequence(obj, nestingLevel);
    }

    private string GetPrintedSequence(IEnumerable obj, int nestingLevel)
    {
        var sb = new StringBuilder();
        var indentation = GetIndentation(nestingLevel);
        var index = 0;
        foreach (var element in obj)
            sb.Append($"{indentation}{index++}: {PrintToString(element, nestingLevel + 1)}");

        return sb.ToString();
    }

    private string GetPrintedDictionary(IDictionary dict, int nestingLevel)
    {
        var sb = new StringBuilder();
        var indentation = GetIndentation(nestingLevel);
        foreach (DictionaryEntry pair in dict)
            sb.Append($"{indentation}{pair.Key}: {PrintToString(pair.Value, nestingLevel + 1)}");

        return sb.ToString();
    }

    private string GetPrintingResult(Object obj, MemberInfo memberInfo, int nestingLevel)
    {
        string printingResult;

        if (config.CustomTypeSerializers.TryGetValue(GetMemberType(memberInfo), out var typeSerializer))
            printingResult = typeSerializer.DynamicInvoke(GetMemberValue(memberInfo, obj)) + Environment.NewLine;
        else if (config.CustomMemberSerializers.TryGetValue(memberInfo, out var propertySerializer))
            printingResult = propertySerializer.DynamicInvoke(GetMemberValue(memberInfo, obj)) + Environment.NewLine;
        else
            printingResult = PrintToString(GetMemberValue(memberInfo, obj),
                nestingLevel + 1);

        return TrimResultIfNeeded(memberInfo, printingResult);
    }

    private string TrimResultIfNeeded(MemberInfo memberInfo, string printingResult)
    {
        if (config.TrimmedMembers.TryGetValue(memberInfo, out var length))
            printingResult = printingResult.Length >= length
                ? printingResult[..length] + Environment.NewLine
                : printingResult;

        return printingResult;
    }

    private static object GetMemberValue(MemberInfo member, Object obj)
    {
        if (!IsPropertyOrField(member))
            throw new ArgumentException("Provided member must be Field or Property");

        return member.MemberType == MemberTypes.Field
            ? ((FieldInfo)member).GetValue(obj)
            : ((PropertyInfo)member).GetValue(obj);
    }

    private static Type GetMemberType(MemberInfo member)
    {
        if (!IsPropertyOrField(member))
            throw new ArgumentException("Provided member must be Field or Property");

        return member.MemberType == MemberTypes.Field
            ? ((FieldInfo)member).FieldType
            : ((PropertyInfo)member).PropertyType;
    }

    private static bool IsPropertyOrField(MemberInfo member)
    {
        return member.MemberType is MemberTypes.Field or MemberTypes.Property;
    }

    private static bool IsTypeFinal(Type type)
    {
        return type.IsPrimitive || type == typeof(string) || type == typeof(Guid);
    }

    private static string GetIndentation(int nestingLevel) => new string('\t', nestingLevel + 1);
}