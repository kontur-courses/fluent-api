using System;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting;

public class NumericPropertyConfig<TOwner, TProperty>(
    PrintingConfig<TOwner> printingConfig,
    ObjectSerializer objectSerializer,
    Expression<Func<TOwner, TProperty>> selector)
{
    /// <summary>
    /// Устанавливает культуру для форматирования чисел
    /// </summary>
    /// <param name="cultureInfo">Используемая культура</param>
    /// <exception cref="ArgumentNullException">Если <paramref name="cultureInfo"/> равен <c>null</c></exception>
    public PrintingConfig<TOwner> SetNumericCulture(
        CultureInfo cultureInfo)
    {
        ArgumentNullException.ThrowIfNull(cultureInfo);

        objectSerializer.SetNumericCulture(selector.GetMemberInfo(), cultureInfo);

        return printingConfig;
    }
}