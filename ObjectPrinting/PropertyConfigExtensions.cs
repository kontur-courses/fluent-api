using System;
using System.Linq.Expressions;

namespace ObjectPrinting;

public static class PropertyConfigExtensions
{
    /// <summary>
    /// Исключает из вывода конкретное свойство или поле, заданное лямбдой
    /// </summary>
    /// <param name="selector">Прямое обращение к члену, например <c>x => x.Password</c>.</param>
    /// <exception cref="ArgumentNullException">Если <paramref name="selector"/> равен <c>null</c></exception>
    /// <exception cref="ArgumentException">Если выражение не является прямым обращением к полю или свойству</exception>
    public static PrintingConfig<TOwner> Exclude<TOwner, TProperty>(
        this PropertyConfig<TOwner, TProperty> propertyConfig
    )
    {
        ArgumentNullException.ThrowIfNull(propertyConfig);
        
        propertyConfig.ObjectSerializer.AddExcludedMember(propertyConfig.MemberInfo);
        
        return propertyConfig.PrintingConfig;
    }
    
    /// <summary>
    /// Задаёт кастомный сериализатор для конкретного члена. Имеет приоритет над сериализатором типа
    /// </summary>
    /// <typeparam name="TMember">Тип члена</typeparam>
    /// <param name="memberSelector">Прямое обращение к члену, например <c>x => x.Name</c></param>
    /// <param name="serializer">Функция преобразования значения в строку</param>
    /// <exception cref="ArgumentNullException">Если <paramref name="serializer"/> равен <c>null</c></exception>
    /// <exception cref="ArgumentException">Если выражение не является прямым обращением к полю или свойству</exception>
    public static PrintingConfig<TOwner> SetModifiedSerialization<TOwner, TMember>(
        this PropertyConfig<TOwner, TMember> propertyConfig,
        Func<TMember, string> serializer)
    {
        ArgumentNullException.ThrowIfNull(propertyConfig);
        ArgumentNullException.ThrowIfNull(serializer);

        propertyConfig.ObjectSerializer.AddModifiedMemberSerializer(
            propertyConfig.MemberInfo,
            serializer);
        
        return propertyConfig.PrintingConfig;
    }

    /// <summary>
    /// Обрезает строковое значение члена до <paramref name="length"/> символов
    /// </summary>
    /// <param name="selector">Прямое обращение к строковому члену</param>
    /// <param name="length">Максимальная длина строки (≥ 0)</param>
    /// <exception cref="ArgumentOutOfRangeException">Если <paramref name="length"/> отрицателен</exception>
    public static PrintingConfig<TOwner> Truncate<TOwner>(
        this PropertyConfig<TOwner, string> propertyConfig,
        int length)
    {
        ArgumentNullException.ThrowIfNull(propertyConfig);
        ArgumentOutOfRangeException.ThrowIfNegative(length);
            
        return propertyConfig.SetModifiedSerialization(
            value => value[..Math.Min(length, value.Length)]);
    }
}