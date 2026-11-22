using System;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting;

/// <summary>
/// Настраивает правила сериализации объекта <typeparamref name="TOwner"/>
/// в строку: исключение членов/типов, кастомные сериализаторы,
/// культуру чисел и обрезку строк
/// </summary>
public class PrintingConfig<TOwner>
{
    private readonly ObjectSerializer _objectSerializer = new();

    /// <summary>
    /// Сериализует <paramref name="obj"/> в строку с учётом настроек
    /// </summary>
    /// <param name="obj">Объект для сериализации</param>
    /// <returns>Строковое представление сериализованного объекта</returns>
    public string PrintToString(TOwner obj)
    {
        return _objectSerializer.SerializeObject(obj);
    }

    /// <summary>
    /// Исключает из вывода все члены типа <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">Тип, члены которого не должны попадать в результат</typeparam>
    public PrintingConfig<TOwner> Exclude<T>()
    {
        _objectSerializer.AddExcludedType(typeof(T));
        return this;
    }

    /// <summary>
    /// Задаёт кастомный сериализатор для всех членов типа <typeparamref name="TMember"/>.
    /// Применяется, если для конкретного члена не задан кастомный сериализатор
    /// </summary>
    /// <typeparam name="TMember">Тип, для которого задаётся сериализатор</typeparam>
    /// <param name="serializer">Функция преобразования значения в строку</param>
    /// <exception cref="ArgumentNullException">Если <paramref name="serializer"/> равен <c>null</c></exception>
    public PrintingConfig<TOwner> SetTypeSerialization<TMember>(Func<TMember, string> serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);

        _objectSerializer.AddModifiedTypeSerializer<TMember>(serializer);
        return this;
    }
    
    public PropertyConfig<TOwner, TProperty> ForProperty<TProperty>(
        Expression<Func<TOwner, TProperty>> selector)
    {
        return new PropertyConfig<TOwner, TProperty>(
            this, _objectSerializer, selector);
    }
}