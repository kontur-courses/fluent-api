using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using ObjectPrinting.Configuration;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        internal HashSet<Type> excludedTypes { get; } = new();
        internal HashSet<string> excludedProperties { get; } = new();
        internal Dictionary<Type, Func<object, string>> typeSerializers { get; } = new();
        internal Dictionary<string, Func<object, string>> propertySerializers { get; } = new();
        internal Dictionary<Type, CultureInfo> typeCultures { get; } = new();
        internal Dictionary<string, int> trimLengths { get; } = new();

        public PrintingConfig<TOwner> Exclude<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> Using<T>(Func<T, string> serializer)
        {
            typeSerializers[typeof(T)] = obj => serializer((T)obj);
            return this;
        }

        public PrintingConfig<TOwner> UsingCulture<T>(CultureInfo culture)
        {
            typeCultures[typeof(T)] = culture;
            return this;
        }

        public IPropertyConfig<TOwner> Property(string propertyName)
        {
            return new PropertyConfig<TOwner>(this, propertyName);
        }

        public IStringPropertyConfig<TOwner> Property(Expression<Func<TOwner, string>> propertySelector)
        {
            var name = GetPropertyName(propertySelector);
            return new StringPropertyConfig<TOwner>(this, name);
        }

        public IPropertyConfig<TOwner> Property(Expression<Func<TOwner, object>> propertySelector)
        {
            var name = GetPropertyName(propertySelector);
            return new PropertyConfig<TOwner>(this, name);
        }

        public string PrintToString(TOwner obj)
        {
            var serializer = new ObjectSerializer<TOwner>(this);
            return serializer.Serialize(obj);
        }

        private string GetPropertyName(LambdaExpression selector)
        {
            var body = selector.Body;

            if (body is UnaryExpression unary)
                body = unary.Operand;

            if (body is MemberExpression member)
                return member.Member.Name;

            throw new ArgumentException("Expression must be a property access");
        }
    }
}