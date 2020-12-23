using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public ImmutableHashSet<Type> ExcludedTypes { get; set; }
        public ImmutableHashSet<PropertyInfo> ExcludedProperties { get; set; }
        public ImmutableDictionary<Type, Delegate> AltSerializerForType { get; set; }
        public ImmutableDictionary<PropertyInfo, Delegate> AltSerializerForProperty { get; set; }
        public ImmutableDictionary<Type, CultureInfo> CultureForType { get; set; }

        public ObjectPrinter<TOwner> Build()
        {
            return new ObjectPrinter<TOwner>(this);
        }

        public PrintingConfig()
        {
            ExcludedTypes = ImmutableHashSet.Create<Type>();
            ExcludedProperties = ImmutableHashSet.Create<PropertyInfo>();
            AltSerializerForType = ImmutableDictionary.Create<Type, Delegate>();
            AltSerializerForProperty = ImmutableDictionary.Create<PropertyInfo, Delegate>();
            CultureForType = ImmutableDictionary.Create<Type, CultureInfo>();
        }

        public PrintingConfig(PrintingConfig<TOwner> previousConfig)
        {
            ExcludedTypes = previousConfig.ExcludedTypes;
            ExcludedProperties = previousConfig.ExcludedProperties;
            AltSerializerForType = previousConfig.AltSerializerForType;
            AltSerializerForProperty = previousConfig.AltSerializerForProperty;
            CultureForType = previousConfig.CultureForType;
        }

        public PrintingConfig<TOwner> Exclude<TExcluded>()
        {
            var newConfig = new PrintingConfig<TOwner>(this);
            newConfig.ExcludedTypes = ExcludedTypes.Add(typeof(TExcluded));
            return newConfig;
        }

        public PrintingConfig<TOwner> Exclude<TProperty>(Expression<Func<TOwner, TProperty>> property)
        {
            var newConfig = new PrintingConfig<TOwner>(this);
            var propertyInfo = ((MemberExpression)property.Body).Member as PropertyInfo;
            newConfig.ExcludedProperties = ExcludedProperties.Add(propertyInfo);
            return newConfig;
        }

        public PrintingConfig<TOwner> SetSerializerFor<T>(Func<T, string> serializer)
        {
            var newConfig = new PrintingConfig<TOwner>(this);
            newConfig.AltSerializerForType = AltSerializerForType.AddOrUpdate(typeof(T), serializer);
            return newConfig;
        }

        public PrintingConfig<TOwner> SetCultureFor<T>(CultureInfo culture)
        {
            if (typeof(IFormattable).IsAssignableFrom(typeof(T)))
            {
                var newConfig = new PrintingConfig<TOwner>(this);
                newConfig.CultureForType = CultureForType.AddOrUpdate(typeof(T), culture);
                return newConfig;
            }

            throw new ArgumentException("Невозможно установить культуру для данного типа");
        }

        public PropertyPrintingConfig<TOwner, TProperty> ForProperty<TProperty>(Expression<Func<TOwner, TProperty>> property)
        {
            var propertyInfo = ((MemberExpression) property.Body).Member as PropertyInfo;
            return new PropertyPrintingConfig<TOwner, TProperty>(propertyInfo, this);
        }
    }
}
