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
    /// Исключает из вывода конкретное свойство или поле, заданное лямбдой
    /// </summary>
    /// <param name="selector">Прямое обращение к члену, например <c>x => x.Password</c>.</param>
    /// <exception cref="ArgumentNullException">Если <paramref name="selector"/> равен <c>null</c></exception>
    /// <exception cref="ArgumentException">Если выражение не является прямым обращением к полю или свойству</exception>
    public PrintingConfig<TOwner> Exclude(Expression<Func<TOwner, object>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);

        _objectSerializer.AddExcludedMember(selector.GetMemberInfo());
        return this;
    }

    /// <summary>
    /// Задаёт кастомный сериализатор для конкретного члена. Имеет приоритет над сериализатором типа
    /// </summary>
    /// <typeparam name="TMember">Тип члена</typeparam>
    /// <param name="memberSelector">Прямое обращение к члену, например <c>x => x.Name</c></param>
    /// <param name="serializer">Функция преобразования значения в строку</param>
    /// <exception cref="ArgumentNullException">Если <paramref name="serializer"/> равен <c>null</c></exception>
    /// <exception cref="ArgumentException">Если выражение не является прямым обращением к полю или свойству</exception>
    public PrintingConfig<TOwner> SetModifiedSerialization<TMember>
        (Expression<Func<TOwner, TMember>> memberSelector, Func<TMember, string> serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);

        var memberInfo = memberSelector.GetMemberInfo();
        _objectSerializer.AddModifiedMemberSerializer(memberInfo, serializer);
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
    
    /// <summary>
    /// Обрезает строковое значение члена до <paramref name="length"/> символов
    /// </summary>
    /// <param name="selector">Прямое обращение к строковому члену</param>
    /// <param name="length">Максимальная длина строки (≥ 0)</param>
    /// <exception cref="ArgumentOutOfRangeException">Если <paramref name="length"/> отрицателен</exception>
    public PrintingConfig<TOwner> Truncate(Expression<Func<TOwner, string>> selector, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        return SetModifiedSerialization(
            selector,
            value => value[..Math.Min(length, value.Length)]);
    }
    
    public NumericPropertyConfig<TOwner, double> ForProperty
        (Expression<Func<TOwner, double>> selector)
    {
        var propertyConfig = new NumericPropertyConfig<TOwner, double>(
            this,
            _objectSerializer,
            selector
        );
        return propertyConfig;
    }
    
    public NumericPropertyConfig<TOwner, int> ForProperty(
        Expression<Func<TOwner, int>> selector)
    {
        return new NumericPropertyConfig<TOwner, int>(
            this,
            _objectSerializer,
            selector);
    }
    
    public PropertyConfig<TOwner, TProperty> ForProperty<TProperty>(
        Expression<Func<TOwner, TProperty>> selector)
    {
        return new PropertyConfig<TOwner, TProperty>(
            this, _objectSerializer, selector);
    }
}