using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly Configurations configurations;
        private readonly Type[] finalTypes;

        public PrintingConfig()
        {
            configurations = new Configurations();
            finalTypes = new Type[] 
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
        }

        public PrintingConfig<TOwner> Excluding<TProperty>()
        {
            configurations.ExcludedTypes.Add(typeof(TProperty));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TProperty>(Expression<Func<TOwner, TProperty>> expression)
        {
            if (expression.Body is MemberExpression memberExpression)
            if (memberExpression.Member is PropertyInfo propertyInfo)
            {
                configurations.ExcludedProperties.Add(propertyInfo);
                return this;
            }

            throw new ArgumentException();
        }

        public PropertyPrintingConfig<TOwner, TProperty> Printing<TProperty>()
        {
            return new PropertyPrintingConfig<TOwner, TProperty>(this, configurations);
        }

        public PropertyPrintingConfig<TOwner, TProperty> Printing<TProperty>(
            Expression<Func<TOwner, TProperty>> expression)
        {
            if (expression.Body is MemberExpression memberExpression)
            if (memberExpression.Member is PropertyInfo propertyInfo)
            {
                return new PropertyPrintingConfig<TOwner, TProperty>(
                    this, configurations, propertyInfo);
            }

            throw new ArgumentException();
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj is null)
                return "null";

            var type = obj.GetType();
            var culture = configurations.Cultures.GetValueOrDefault(type);

            if (finalTypes.Contains(type))
                return Convert.ToString(obj, culture);

            var identation = new string('\t', nestingLevel + 1);
            var stringBuilder = new StringBuilder().AppendLine(type.Name);

            var properties = type.GetProperties()
                .Where(prop => !configurations.ExcludedTypes.Contains(prop.PropertyType))
                .Where(prop => !configurations.ExcludedProperties.Contains(prop));

            foreach (var propertyInfo in properties)
            {
                var serializedProperty = SerializeProperty(propertyInfo, obj);
                var line = string.Format("{0}{1} = {2}",
                    identation, propertyInfo.Name, PrintToString(serializedProperty, nestingLevel + 1));
                stringBuilder.AppendLine(line);
            }

            return stringBuilder.ToString();
        }

        private object SerializeProperty(PropertyInfo propertyInfo, object obj)
        {
            Func<object, object> defaultPrint = (value) => value;
            var print = configurations.SerializationOfProperties.GetValueOrDefault(propertyInfo)
                ?? configurations.SerializationOfTypes.GetValueOrDefault(propertyInfo.PropertyType)
                ?? TryGetCollectionPrint(propertyInfo)
                ?? defaultPrint;

            var value = propertyInfo.GetValue(obj);
            if (value is null) return null;
            return print.Method.Invoke(print.Target, new object[] { value });
        }

        private Func<object, object> TryGetCollectionPrint(PropertyInfo propertyInfo)
        {
            if (IsCollection(propertyInfo.PropertyType))
            {
                return (collection) =>
                {
                    var elements = string.Join(", ", (collection as ICollection).Cast<object>());
                    if (IsDictionary(propertyInfo.PropertyType))
                    {
                        var keyValuePairs = (collection as IDictionary).Cast()
                            .Select(entry => $"[{entry.Key}] = {entry.Value}");
                        elements = string.Join(", ", keyValuePairs);
                    }
                    return $"[{elements}]";
                };
            }
            return null;
        }

        private bool IsDictionary(Type type) => typeof(IDictionary).IsAssignableFrom(type);
        private bool IsCollection(Type type) => typeof(ICollection).IsAssignableFrom(type);
    }
}