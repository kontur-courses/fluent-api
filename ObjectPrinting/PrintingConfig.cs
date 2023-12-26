using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    private readonly HashSet<Type> excludedTypes = new();
    private readonly HashSet<MemberInfo> excludedMembers = new();

    private HashSet<object> printedObjects;

    internal Dictionary<Type, Delegate> CustomTypeSerializers { get; } = new();

    internal Dictionary<MemberInfo, Delegate> CustomMemberSerializers { get; } = new();

    internal Dictionary<Type, CultureInfo> CulturesForTypes { get; } = new();

    internal Dictionary<MemberInfo, int> TrimmedMembers { get; } = new();

    public MemberPrintingConfig<TOwner, TMemberType> SetPrintingFor<TMemberType>()
    {
        return new MemberPrintingConfig<TOwner, TMemberType>(this);
    }

    public MemberPrintingConfig<TOwner, TMemberType> SetPrintingFor<TMemberType>(
        Expression<Func<TOwner, TMemberType>> memberSelector)
    {
        var expression = (MemberExpression)memberSelector.Body;
        return new MemberPrintingConfig<TOwner, TMemberType>(this, expression.Member);
    }

    public PrintingConfig<TOwner> ExcludeMember<TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
    {
        var expression = (MemberExpression)memberSelector.Body;
        excludedMembers.Add(expression.Member);

        return this;
    }

    public PrintingConfig<TOwner> ExcludeMemberType<TMemberType>()
    {
        excludedTypes.Add(typeof(TMemberType));

        return this;
    }

    public string PrintToString(TOwner obj)
    {
        printedObjects = new HashSet<object>();

        return PrintToString(obj, 0);
    }

    private string PrintToString(object obj, int nestingLevel)
    {
        if (obj == null)
            return "null" + Environment.NewLine;

        if (!obj.GetType().IsValueType && printedObjects.Contains(obj))
            return "Cycle reference detected" + Environment.NewLine;

        printedObjects.Add(obj);

        var indentation = GetIndentation(nestingLevel);

        if (CulturesForTypes.TryGetValue(obj.GetType(), out var culture))
            return ((IFormattable)obj).ToString(null, culture) + Environment.NewLine;

        if (IsTypeFinal(obj.GetType()))
            return obj + Environment.NewLine;

        var sb = new StringBuilder();
        var type = obj.GetType();
        sb.AppendLine(type.Name);

        if (obj is IEnumerable enumerable)
            sb.Append(GetPrintedCollection(enumerable, nestingLevel));
        else
            foreach (var memberInfo in type.GetMembers().Where(IsPropertyOrField))
            {
                if (excludedTypes.Contains(GetMemberType(memberInfo)) || excludedMembers.Contains(memberInfo))
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

        if (CustomTypeSerializers.TryGetValue(GetMemberType(memberInfo), out var typeSerializer))
            printingResult = typeSerializer.DynamicInvoke(GetMemberValue(memberInfo, obj)) + Environment.NewLine;
        else if (CustomMemberSerializers.TryGetValue(memberInfo, out var propertySerializer))
            printingResult = propertySerializer.DynamicInvoke(GetMemberValue(memberInfo, obj)) + Environment.NewLine;
        else
            printingResult = PrintToString(GetMemberValue(memberInfo, obj),
                nestingLevel + 1);

        return TrimResultIfNeeded(memberInfo, printingResult);
    }

    private string TrimResultIfNeeded(MemberInfo memberInfo, string printingResult)
    {
        if (TrimmedMembers.TryGetValue(memberInfo, out var length))
            printingResult = printingResult[..length] + Environment.NewLine;

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
        return type.IsValueType || type == typeof(string);
    }

    private static string GetIndentation(int nestingLevel) => new string('\t', nestingLevel + 1);
}