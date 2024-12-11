using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectSerializer.Interfaces;

namespace ObjectSerializer.ConcreteConfigs;

public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
{
    public HashSet<MemberInfo> ExcludedMemberInfos = new();
    public HashSet<Type> ExcludedTypes = new();

    public Dictionary<MemberInfo, Func<object, string>> MemberInfosPrintFunc = new();

    public Dictionary<Type, CultureInfo> TypeCultureInfos = new();

    public Dictionary<Type, Func<object, string>> TypePrintFunc = new();

    #region HelpersMethods

    private MemberInfo GetMemberInfo<TSomeField>(Expression<Func<TOwner, TSomeField>> selectMember)
    {
        if (selectMember.Body is not MemberExpression memberExpression)
            throw new ArgumentException();

        return memberExpression.Member;
    }

    #endregion

    #region For methods

    public ITypePrintConfig<TOwner, TType> For<TType>()
    {
        return new TypePrintConfig<TOwner, TType>(this);
    }

    public IFieldPrintConfig<TOwner, TField> For<TField>(Expression<Func<TOwner, TField>> selectMember)
    {
        var memberInfo = GetMemberInfo(selectMember);

        return new FieldPrintConfig<TOwner, TField>(this, memberInfo);
    }

    #endregion

    #region ExcludeMethods

    public IPrintingConfig<TOwner> Exclude<TType>()
    {
        ExcludedTypes.Add(typeof(TType));

        return this;
    }

    public IPrintingConfig<TOwner> Exclude<TField>(Expression<Func<TOwner, TField>> selectMember)
    {
        var memberInfo = GetMemberInfo(selectMember);

        ExcludedMemberInfos.Add(memberInfo);

        return this;
    }

    #endregion

    #region PrintToString

    public string PrintToString(TOwner obj)
    {
        if (obj == null) 
            throw new ArgumentNullException(nameof(obj));

        return PrintToString(obj, 0);
    }

    private string PrintToString(object obj, int nestingLevel)
    {
        //TODO apply configurations
        if (obj == null)
            return "null" + Environment.NewLine;

        var finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };
        if (finalTypes.Contains(obj.GetType()))
            return obj + Environment.NewLine;

        var identation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        var type = obj.GetType();
        sb.AppendLine(type.Name);
        foreach (var propertyInfo in type.GetProperties())
            sb.Append(identation + propertyInfo.Name + " = " +
                      PrintToString(propertyInfo.GetValue(obj),
                          nestingLevel + 1));
        return sb.ToString();
    }

    #endregion
}