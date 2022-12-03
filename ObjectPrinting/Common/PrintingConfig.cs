using NUnit.Framework.Constraints;
using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Common
{
    public interface IPrintingConfig<TOwner> : IHaveRoot
    {
        public string PrintToString(TOwner obj)
        {
            return ObjectSerializer.Serialize(obj, Root);
        }

        public IPrintingConfig<TOwner> Exclude<TExcluded>()
        {
            Root.ExcludedTypes.Add(typeof(TExcluded));
            return this;
        }

        public IPrintingConfig<TOwner> SerializeTypeAs<T>(Func<T, string> serializationFunction)
        {
            var type = typeof(T);
            if (!Root.TypeSerializers.ContainsKey(type))
                Root.TypeSerializers.Add(type, obj => serializationFunction((T)obj));
            return this;
        }

        public IPrintingConfig<TOwner> SetNumericTypeCulture<T>(CultureInfo culture) where T : IConvertible
        {
            var type = typeof(T);
            if (!Root.NumericTypeCulture.ContainsKey(type))
                Root.NumericTypeCulture.Add(type, culture);

            return this;
        }

        public IPrintingPropertyBaseConfig<TOwner, T> ConfigurePropertySerialization<T>(Expression<Func<TOwner, T>> property)
        {
            var propertyInfo = GetPropertyInfoFromExpression(property.Body);
            return new PrintingPropertyBaseConfig<TOwner, T>(propertyInfo, Root);
        }

        public IPrintingConfig<TOwner> Exclude<T>(Expression<Func<TOwner, T>> property)
        {
            var propertyInfo = GetPropertyInfoFromExpression(property.Body);
            Root.ExcludedProperties.Add(propertyInfo);
            return this;
        }

        private static MemberInfo GetPropertyInfoFromExpression(Expression expression)
        {
            if (!(expression is MemberExpression memberExpression))
                throw new ArgumentException($"Can't find property {expression}");
            return memberExpression.Member;
        }
    }

    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        PrintingConfigRoot IHaveRoot.Root { get; } = new PrintingConfigRoot();
    }
}