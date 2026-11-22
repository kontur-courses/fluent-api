using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly PrintingSettings settings = new();
        private readonly ObjectWriter objectWriter;

        public PrintingConfig()
        {
            objectWriter = new ObjectWriter(settings);
        }

        public PrintingConfig<TOwner> Excluding<TValue>()
        {
            settings.SetExcludingType<TValue>();
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TValue>(Expression<Func<TOwner, TValue>> expression)
        {
            settings.SetExcludingMember(GetSelectedMember(expression));
            return this;
        }

        public TypeConfig<TOwner, TValue> Configure<TValue>()
        {
            return new TypeConfig<TOwner, TValue>(this, settings);
        }

        public MemberConfig<TOwner, TValue> Configure<TValue>(
            Expression<Func<TOwner, TValue>> expression)
        {
            return new MemberConfig<TOwner, TValue>(
                this,
                settings,
                GetSelectedMember(expression));
        }

        public StringMemberConfig<TOwner> Configure(
            Expression<Func<TOwner, string>> expression)
        {
            return new StringMemberConfig<TOwner>(
                this,
                settings,
                GetSelectedMember(expression));
        }

        public string PrintToString(TOwner obj)
        {
            return objectWriter.Print(obj);
        }

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
