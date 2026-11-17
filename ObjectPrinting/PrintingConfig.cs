using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

public class PrintingConfig<TOwner>
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly Type[] FinalTypes =
    [
        typeof(string),
        typeof(decimal),
        typeof(Guid),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(TimeSpan)
    ];

    private readonly HashSet<Type> _excludedTypes = [];
    private readonly HashSet<MemberInfo> _excludedMembers = [];
    private readonly Dictionary<Type, Func<object, string>> _typePrinters = [];
    private readonly Dictionary<MemberInfo, Func<object, string>> _memberPrinters = [];

    public string PrintToString(TOwner obj)
    {
        return PrintToString(obj, 0);
    }

    public PrintingConfig<TOwner> Exclude<TExcludingType>()
    {
        ExcludeType(typeof(TExcludingType));
        return this;
    }

    public PrintingConfig<TOwner> Exclude<TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
    {
        ExcludeMember(GetSelectedMember(memberSelector));
        return this;
    }

    public ValuePrintingConfig<TOwner, TPrintingType> Printing<TPrintingType>()
    {
        return new ValuePrintingConfig<TOwner, TPrintingType>(
            this,
            printer => _typePrinters[typeof(TPrintingType)] = printer);
    }

    public ValuePrintingConfig<TOwner, TMemberType> Printing<TMemberType>(
        Expression<Func<TOwner, TMemberType>> memberSelector)
    {
        var member = GetSelectedMember(memberSelector);
        return new ValuePrintingConfig<TOwner, TMemberType>(
            this,
            printer => _memberPrinters[member] = printer);
    }

    internal void ExcludeType(Type excludedType)
    {
        _excludedTypes.Add(excludedType);
    }

    internal void ExcludeMember(MemberInfo excludedMember)
    {
        _excludedMembers.Add(excludedMember);
    }

    private string PrintToString(object obj, int nestingLevel)
    {
        if (obj == null)
            return "null";

        var type = obj.GetType();
        if (_typePrinters.TryGetValue(type, out var typePrinter))
            return typePrinter(obj);

        if (IsFinalType(type))
            return obj.ToString();

        var indentation = new string('\t', nestingLevel + 1);
        var builder = new StringBuilder(type.Name);
        foreach (var propertyInfo in type.GetProperties())
        {
            if (IsExcluded(propertyInfo))
                continue;

            var printedValue = PrintMember(propertyInfo, propertyInfo.GetValue(obj), nestingLevel);
            builder.Append(Environment.NewLine + indentation + propertyInfo.Name + " = " + printedValue);
        }

        return builder.ToString();
    }

    private bool IsExcluded(PropertyInfo propertyInfo)
    {
        return _excludedMembers.Contains(propertyInfo)
            || _excludedTypes.Contains(propertyInfo.PropertyType);
    }

    private string PrintMember(MemberInfo member, object value, int nestingLevel)
    {
        return _memberPrinters.TryGetValue(member, out var memberPrinter)
            ? memberPrinter(value)
            : PrintToString(value, nestingLevel + 1);
    }

    private static MemberInfo GetSelectedMember<TMemberType>(Expression<Func<TOwner, TMemberType>> memberSelector)
    {
        ArgumentNullException.ThrowIfNull(memberSelector);
        var memberExpression = memberSelector.Body as MemberExpression;
        ThrowIfNotOwnerMember(memberExpression, nameof(memberSelector));
        return memberExpression!.Member;
    }

    private static void ThrowIfNotOwnerMember(MemberExpression memberExpression, string paramName)
    {
        if (memberExpression?.Expression is ParameterExpression)
            return;

        throw new ArgumentException("Selector must return a member of the printed object", paramName);
    }

    private static bool IsFinalType(Type type)
    {
        return type.IsPrimitive
            || type.IsEnum
            || FinalTypes.Contains(type);
    }
}