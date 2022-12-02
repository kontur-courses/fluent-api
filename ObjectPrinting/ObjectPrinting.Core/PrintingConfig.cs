using System.Reflection;
using System.Globalization;
using System.Linq.Expressions;
using ObjectPrinting.Core.Configs;
using ObjectPrinting.Core.Configs.Basics;
using ObjectPrinting.Core.Configs.Defaults;
using ObjectPrinting.Core.Configs.Formattable;

namespace ObjectPrinting.Core
{
    public class PrintingConfig<TOwner>
    {
        internal Config Config = new();

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

        public PrintingConfig<TOwner> TrimEnd(Expression<Func<TOwner, string>> propertyAccess, int trimLength)
        {
            return TrimEnd(GetMemberInfo(propertyAccess), trimLength);
        }

        public PrintingConfig<TOwner> WithCulture<T>(Expression<Func<TOwner, T>> propertyAccess, CultureInfo cultureInfo) where T : IFormattable
        {
            return WithCulture(GetMemberInfo(propertyAccess), cultureInfo);
        }

        public PrintingConfig<TOwner> WithSerializer<T>(Expression<Func<TOwner, T>> propertyAccess, Func<T, string> serializer)
        {
            return WithSerializer(GetMemberInfo(propertyAccess), serializer);
        }

        public PrintingConfig<TOwner> Exclude<TProperty>(Expression<Func<TOwner, TProperty>> propertyAccess)
        {
            return Exclude(GetMemberInfo(propertyAccess));
        }

        public PrintingConfig<TOwner> TrimEnd(int trimLength)
        {
            if (trimLength < -1)
                throw new ArgumentOutOfRangeException(nameof(trimLength), "Trim length is negative");

            Config.StringTrimLength = trimLength;
            return this;
        }

        internal PrintingConfig<TOwner> TrimEnd(MemberInfo member, int trimLength)
        {
            if (trimLength < -1)
                throw new ArgumentOutOfRangeException(nameof(trimLength), "Trim length is negative");

            Config.MemberTrimLengths[member] = trimLength;
            return this;
        }

        public PrintingConfig<TOwner> WithCulture<T>(CultureInfo cultureInfo) where T : IFormattable
        {
            Config.TypeCultureSettings[typeof(T)] = cultureInfo;
            return this;
        }

        public PrintingConfig<TOwner> WithCulture(MemberInfo member, CultureInfo cultureInfo) 
        {
            Config.MemberCultureSettings[member] = cultureInfo;
            return this;
        }

        public PrintingConfig<TOwner> WithSerializer<T>(Func<T, string> serializer)
        {
            Config.TypeSpecificSerializers[typeof(T)] = x => serializer((T)x);
            return this;
        }

        internal PrintingConfig<TOwner> WithSerializer<T>(MemberInfo member, Func<T, string> serializer)
        {
            Config.MemberSpecificSerializers[member] = x => serializer((T)x);
            return this;
        }

        public PrintingConfig<TOwner> Exclude<T>()
        {
            Config.ExcludedTypes.Add(typeof(T));
            return this;
        }

        internal PrintingConfig<TOwner> Exclude(MemberInfo member)
        {
            Config.ExcludedMembers.Add(member);
            return this;
        }

        private static MemberInfo GetMemberInfo<TObject, TMember>(Expression<Func<TObject, TMember>> memberAccess)
        {
            if (memberAccess.Body.NodeType != ExpressionType.MemberAccess)
                throw new ArgumentException("Expression does not allow access", nameof(memberAccess));

            return ((MemberExpression)memberAccess.Body).Member;
        }
    }
}