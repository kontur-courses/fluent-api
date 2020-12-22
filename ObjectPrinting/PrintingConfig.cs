using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public ImmutableHashSet<Type> ExcludedTypes { get; set; }
        public ImmutableHashSet<PropertyInfo> ExcludedProperties { get; set; }
        public ImmutableDictionary<Type, Delegate> AltSerializerForType { get; set; }
        public ImmutableDictionary<PropertyInfo, Delegate> AltSerializerForProperty { get; set; }
        public ImmutableDictionary<Type, CultureInfo> CultureForType { get; set; }

        public PrintingConfig()
        {
            ExcludedTypes = ImmutableHashSet.Create<Type>();
            ExcludedProperties = ImmutableHashSet.Create<PropertyInfo>();
            AltSerializerForType = ImmutableDictionary.Create<Type, Delegate>();
            AltSerializerForProperty = ImmutableDictionary.Create<PropertyInfo, Delegate>();
            CultureForType = ImmutableDictionary.Create<Type, CultureInfo>();
        }

        public PrintingConfig(PrintingConfig<TOwner> previousConfig) : this()
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
            newConfig.ExcludedTypes.Add(typeof(TExcluded));
            return newConfig;
        }

        public PrintingConfig<TOwner> SetSerializerFor<T>(Func<T, string> serializer)
        {
            var newConfig = new PrintingConfig<TOwner>(this);
            newConfig.AltSerializerForType.Add(typeof(T), serializer);
            return newConfig;
        }

        public PrintingConfig<TOwner> SetCultureFor<T>(CultureInfo culture)
        {
            if (typeof(IFormattable).IsAssignableFrom(typeof(T)))
            {
                var newConfig = new PrintingConfig<TOwner>(this);
                newConfig.CultureForType.Add(typeof(T), culture);
                return newConfig;
            }

            throw new ArgumentException("Невозможно установить культуру для данного типа");
        }

        public PropertyPrintingConfig<TOwner, TProperty> ForProperty<TProperty>(Expression<Func<TOwner, TProperty>> property)
        {
            var propertyInfo = ((MemberExpression) property.Body).Member as PropertyInfo;
            return new PropertyPrintingConfig<TOwner, TProperty>(propertyInfo, this);
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }
    }

    public class PropertyPrintingConfig<TOwner, TProperty>
    {
        public PrintingConfig<TOwner> ParentPrintingConfig { get; set; }

        public PropertyInfo Property { get; set; }

        public PropertyPrintingConfig(PropertyInfo property, PrintingConfig<TOwner> parentPrintingConfig)
        {
            Property = property;
            ParentPrintingConfig = parentPrintingConfig;
        }

        public PrintingConfig<TOwner> SetSerializer(Func<TProperty, string> serializer)
        {
            var newConfig = new PrintingConfig<TOwner>(ParentPrintingConfig);
            newConfig.AltSerializerForProperty.Add(Property, serializer);
            return newConfig;
        }
    }
}