using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private ConfigState state;
        ConfigState IPrintingConfig.State => state;
        public ObjectPrinter<TOwner> Build()
        {
            return new ObjectPrinter<TOwner>(this);
        }

        public PrintingConfig()
        {
            state = new ConfigState();
        }

        public PrintingConfig(PrintingConfig<TOwner> previousConfig)
        {
            state = new ConfigState(previousConfig.state);
        }

        public PrintingConfig<TOwner> Exclude<TExcluded>()
        {
            var newConfig = new PrintingConfig<TOwner>(this);
            newConfig.state.ExcludedTypes = state.ExcludedTypes.Add(typeof(TExcluded));
            return newConfig;
        }

        public PrintingConfig<TOwner> Exclude<TProperty>(Expression<Func<TOwner, TProperty>> property)
        {
            var newConfig = new PrintingConfig<TOwner>(this);
            var propertyInfo = ((MemberExpression)property.Body).Member as PropertyInfo;
            newConfig.state.ExcludedProperties = state.ExcludedProperties.Add(propertyInfo);
            return newConfig;
        }

        public PrintingConfig<TOwner> SetSerializerFor<T>(Func<T, string> serializer)
        {
            var newConfig = new PrintingConfig<TOwner>(this);
            newConfig.state.AltSerializerForType = state.AltSerializerForType.AddOrUpdate(typeof(T), serializer);
            return newConfig;
        }

        public PrintingConfig<TOwner> SetCultureFor<T>(CultureInfo culture)
            where T : IFormattable
        {
            if (typeof(IFormattable).IsAssignableFrom(typeof(T)))
            {
                var newConfig = new PrintingConfig<TOwner>(this);
                newConfig.state.CultureForType = state.CultureForType.AddOrUpdate(typeof(T), culture);
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

    public interface IPrintingConfig
    {
        ConfigState State { get; }
    }
}
