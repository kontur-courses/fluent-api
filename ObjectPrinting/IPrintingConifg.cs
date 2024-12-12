using System;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting;

public interface IPrintingConfig<TOwner>
{
    IPrintingConfig<TOwner> Exclude<TFieldType>();
    IPrintingConfig<TOwner> Exclude(Expression<Func<TOwner, object>> expression);
    IMemberPrintingConfig<TOwner, TFieldType> Serialize<TFieldType>();
    IMemberPrintingConfig<TOwner, TFieldType> Serialize<TFieldType>(Expression<Func<TOwner, TFieldType>> expression);
    IPrintingConfig<TOwner> SetCultureFor<TFieldType>(CultureInfo cultureInfo);
    IPrintingConfig<TOwner> SetCultureFor(Expression<Func<TOwner, object>> expression, CultureInfo cultureInfo);
    IPrintingConfig<TOwner> TrimString(Expression<Func<TOwner, string>> expression, int length);
    string PrintToString(TOwner obj);
}

