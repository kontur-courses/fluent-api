using System.Linq.Expressions;

namespace ObjectSerializer.Interfaces;

public interface IPrintingConfig<TOwner>
{
    ITypePrintConfig<TOwner, TType> Print<TType>();

    IFieldPrintConfig<TOwner, TField> Print<TField>(Expression<Func<TOwner, TField>> selectMember);

    IPrintingConfig<TOwner> Exclude<TType>();

    IPrintingConfig<TOwner> Exclude<TField>(Expression<Func<TOwner, TField>> selectMember);

    string PrintToString(TOwner obj);
}