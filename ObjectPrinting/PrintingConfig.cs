using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public partial class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly IObjectPrinter objectPrinter;

        public PrintingConfig(IObjectPrinter objectPrinter)
        {
            this.objectPrinter = objectPrinter;
        }

        public string PrintToString(TOwner obj)
        {
            return objectPrinter.PrintToString(obj);
        }

        public PrintingConfig<TOwner> ForProperties<TProperty>(Action<BasicTypeConfig<TProperty, TOwner>> options)
        {
            options(new BasicTypeConfig<TProperty, TOwner>(this));
            return this;
        }

        public PrintingConfig<TOwner> ForProperties<TProperty>(Action<FormattableTypeConfig<TProperty, TOwner>> options)
            where TProperty : IFormattable
        {
            options(new FormattableTypeConfig<TProperty, TOwner>(this));
            return this;
        }

        public PrintingConfig<TOwner> ForProperty<TProperty>(Expression<Func<TOwner, TProperty>> propertyAccess, Action<BasicMemberConfig<TProperty, TOwner>> options)
        {
            return ForProperty(GetMemberInfo(propertyAccess), options);
        }

        public PrintingConfig<TOwner> ForProperty<TProperty>(MemberInfo member, Action<BasicMemberConfig<TProperty, TOwner>> options)
        {
            options(new BasicMemberConfig<TProperty, TOwner>(this, member));
            return this;
        }

        public PrintingConfig<TOwner> ForProperty<TProperty>(Expression<Func<TOwner, TProperty>> propertyAccess, Action<FormattableMemberConfig<TProperty, TOwner>> options)
            where TProperty : IFormattable
        {
            return ForProperty(GetMemberInfo(propertyAccess), options);
        }

        public PrintingConfig<TOwner> ForProperty<TProperty>(MemberInfo member, Action<FormattableMemberConfig<TProperty, TOwner>> options)
            where TProperty : IFormattable
        {
            options(new FormattableMemberConfig<TProperty, TOwner>(this, member));
            return this;
        }

        public PrintingConfig<TOwner> ForProperty(Expression<Func<TOwner, string>> propertyAccess, Action<StringMemberConfig<TOwner>> options)
        {
            return ForProperty(GetMemberInfo(propertyAccess), options);
        }

        public PrintingConfig<TOwner> ForProperty(MemberInfo member, Action<StringMemberConfig<TOwner>> options)
        {
            options(new StringMemberConfig<TOwner>(this, member));
            return this;
        }

        public PrintingConfig<TOwner> WithTrimLength(Expression<Func<TOwner, string>> propertyAccess, int trimLength)
        {
            return WithTrimLength(GetMemberInfo(propertyAccess), trimLength);
        }

        public PrintingConfig<TOwner> WithCulture<T>(Expression<Func<TOwner, T>> propertyAccess, CultureInfo cultureInfo)
            where T : IFormattable
        {
            return WithCulture<T>(GetMemberInfo(propertyAccess), cultureInfo);
        }

        public PrintingConfig<TOwner> WithSerializer<T>(Expression<Func<TOwner, T>> propertyAccess, Func<T, string> serializer)
        {
            return WithSerializer(GetMemberInfo(propertyAccess), serializer);
        }

        public PrintingConfig<TOwner> Exclude<TProperty>(Expression<Func<TOwner, TProperty>> propertyAccess)
        {
            return Exclude(GetMemberInfo(propertyAccess));
        }

        public PrintingConfig<TOwner> WithTrimLength(int trimLength)
        {
            objectPrinter.SetTrimLength(trimLength);
            return this;
        }

        protected PrintingConfig<TOwner> WithTrimLength(MemberInfo member, int trimLength)
        {
            objectPrinter.SetTrimLength(member, trimLength);
            return this;
        }

        public PrintingConfig<TOwner> WithCulture<T>(CultureInfo cultureInfo)
            where T : IFormattable
        {
            objectPrinter.SetCulture(typeof(T), cultureInfo);
            return this;
        }

        protected PrintingConfig<TOwner> WithCulture<T>(MemberInfo member, CultureInfo cultureInfo)
            where T : IFormattable
        {
            objectPrinter.SetCulture(member, cultureInfo);
            return this;
        }

        public PrintingConfig<TOwner> WithSerializer<T>(Func<T, string> serializer)
        {
            objectPrinter.SetSerializer(typeof(T), x => serializer((T)x));
            return this;
        }

        protected PrintingConfig<TOwner> WithSerializer<T>(MemberInfo member, Func<T, string> serializer)
        {
            objectPrinter.SetSerializer(member, x => serializer((T)x));
            return this;
        }

        public PrintingConfig<TOwner> Exclude<T>()
        {
            objectPrinter.Exclude(typeof(T));
            return this;
        }

        protected PrintingConfig<TOwner> Exclude(MemberInfo member)
        {
            objectPrinter.Exclude(member);
            return this;
        }

        private static MemberInfo GetMemberInfo<TObject, TMember>(Expression<Func<TObject, TMember>> memberAccess)
        {
            if (memberAccess.Body.NodeType != ExpressionType.MemberAccess)
                throw new ArgumentException("Expression should represent member access", nameof(memberAccess));
            return ((MemberExpression)memberAccess.Body).Member;
        }

        void IPrintingConfig<TOwner>.Exclude(MemberInfo member)
        {
            Exclude(member);
        }

        void IPrintingConfig<TOwner>.WithSerializer<TProperty>(MemberInfo member, Func<TProperty, string> serializer)
        {
            WithSerializer(member, serializer);
        }

        void IPrintingConfig<TOwner>.WithTrimLength(int length)
        {
            WithTrimLength(length);
        }

        void IPrintingConfig<TOwner>.Exclude<T>()
        {
            Exclude<T>();
        }

        void IPrintingConfig<TOwner>.WithSerializer<T>(Func<T, string> serializer)
        {
            WithSerializer(serializer);
        }

        void IPrintingConfig<TOwner>.WithCulture<TProperty>(MemberInfo member, CultureInfo cultureInfo)
        {
            WithCulture<TProperty>(member, cultureInfo);
        }

        void IPrintingConfig<TOwner>.WithTrimLength(MemberInfo member, int length)
        {
            WithTrimLength(member, length);
        }

        void IPrintingConfig<TOwner>.WithCulture<TProperty>(CultureInfo cultureInfo)
        {
            WithCulture<TProperty>(cultureInfo);
        }
    }
}