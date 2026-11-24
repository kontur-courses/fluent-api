using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    /// <summary>Позволяет настроить печать объектов и применить заданные правила</summary>
    /// <typeparam name="TOwner">Тип печатаемого объекта</typeparam>
    public class PrintingConfig<TOwner>
    {
        private readonly PrintingSettings settings = new();
        private readonly ObjectWriter objectWriter;

        /// <summary>Создаёт конфигурацию с настройками по умолчанию</summary>
        public PrintingConfig()
        {
            objectWriter = new ObjectWriter(settings);
        }

        /// <summary>Исключает из печати свойства и поля, объявленные с указанным типом</summary>
        /// <typeparam name="TValue">Тип исключаемых свойств и полей</typeparam>
        /// <returns>Текущая конфигурация печати</returns>
        public PrintingConfig<TOwner> Excluding<TValue>()
        {
            settings.SetExcludingType<TValue>();
            return this;
        }

        /// <summary>Исключает из печати выбранное свойство или поле</summary>
        /// <typeparam name="TValue">Тип выбранного свойства или поля</typeparam>
        /// <param name="expression">Выражение, выбирающее свойство или поле, например <c>x =&gt; x.Name</c></param>
        /// <returns>Текущая конфигурация печати</returns>
        /// <exception cref="ArgumentNullException">Параметр <paramref name="expression"/> равен <see langword="null"/></exception>
        /// <exception cref="ArgumentException">Выражение не выбирает свойство или поле непосредственно у параметра</exception>
        public PrintingConfig<TOwner> Excluding<TValue>(Expression<Func<TOwner, TValue>> expression)
        {
            settings.SetExcludingMember(GetSelectedMember(expression));
            return this;
        }

        /// <summary>Открывает настройки печати значений указанного типа</summary>
        /// <typeparam name="TValue">Тип настраиваемых значений</typeparam>
        /// <returns>Настройки сериализатора и культуры для указанного типа</returns>
        public TypeConfig<TOwner, TValue> Configure<TValue>()
        {
            return new TypeConfig<TOwner, TValue>(this, settings);
        }

        /// <summary>Открывает настройки печати выбранного свойства или поля</summary>
        /// <typeparam name="TValue">Тип выбранного свойства или поля</typeparam>
        /// <param name="expression">Выражение, выбирающее свойство или поле, например <c>x =&gt; x.Age</c></param>
        /// <returns>Конфигурация печати выбранного свойства или поля</returns>
        /// <exception cref="ArgumentNullException">Параметр <paramref name="expression"/> равен <see langword="null"/></exception>
        /// <exception cref="ArgumentException">Выражение не выбирает свойство или поле непосредственно у параметра</exception>
        public MemberConfig<TOwner, TValue> Configure<TValue>(
            Expression<Func<TOwner, TValue>> expression)
        {
            return new MemberConfig<TOwner, TValue>(
                this,
                settings,
                GetSelectedMember(expression));
        }

        /// <summary>Открывает настройки печати и обрезки выбранного строкового свойства или поля</summary>
        /// <param name="expression">Выражение, выбирающее строковое свойство или поле, например <c>x =&gt; x.Name</c></param>
        /// <returns>Конфигурация печати выбранного строкового свойства или поля</returns>
        /// <exception cref="ArgumentNullException">Параметр <paramref name="expression"/> равен <see langword="null"/></exception>
        /// <exception cref="ArgumentException">Выражение не выбирает свойство или поле непосредственно у параметра</exception>
        public StringMemberConfig<TOwner> Configure(
            Expression<Func<TOwner, string>> expression)
        {
            return new StringMemberConfig<TOwner>(
                this,
                settings,
                GetSelectedMember(expression));
        }

        /// <summary>Печатает объект с учётом текущих настроек</summary>
        /// <param name="obj">Объект для печати</param>
        /// <returns>Строковое представление объекта</returns>
        public string PrintToString(TOwner obj)
        {
            return objectWriter.Print(obj);
        }

        /// <summary>Извлекает свойство или поле, выбранное непосредственно у параметра выражения</summary>
        private static MemberInfo GetSelectedMember<TValue>(
            Expression<Func<TOwner, TValue>> expression)
        {
            ArgumentNullException.ThrowIfNull(expression);

            if (expression.Body is MemberExpression selectedMember
                && selectedMember.Expression == expression.Parameters[0]
                && (selectedMember.Member is PropertyInfo
                    || selectedMember.Member is FieldInfo))
            {
                return selectedMember.Member;
            }

            throw new ArgumentException(
                "Ожидался выбор свойства или поля на входе",
                nameof(expression));
        }
    }

}
