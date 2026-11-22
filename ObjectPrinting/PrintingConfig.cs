using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes = [];
        private readonly HashSet<PropertyInfo> excludedProperties = [];

        private readonly Dictionary<Type, Func<object, string>> formattersByType = new();
        private readonly Dictionary<PropertyInfo, Func<object, string>> formattersByProperty = new();


        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, GetProperty(memberSelector));
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludedProperties.Add(GetProperty(memberSelector));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object? obj, int nestingLevel)
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

            var type = obj.GetType();


            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedProperties.Contains(propertyInfo) || excludedTypes.Contains(propertyInfo.PropertyType))
                    continue;

                var value = propertyInfo.GetValue(obj);
                string printedValue;

                if (value is not null && TryGetFormatter(propertyInfo, out var formatter))
                    printedValue = formatter(value) + Environment.NewLine;
                else
                    printedValue = PrintToString(value, nestingLevel + 1);

                sb.Append(identation + propertyInfo.Name + " = " + printedValue);
            }

            return sb.ToString();
        }

        private bool TryGetFormatter(PropertyInfo propertyInfo, [NotNullWhen(true)] out Func<object, string>? formatter)
        {
            return formattersByProperty.TryGetValue(propertyInfo, out formatter)
                   || formattersByType.TryGetValue(propertyInfo.PropertyType, out formatter);
        }


        private static PropertyInfo GetProperty<TPropType>(Expression<Func<TOwner, TPropType>> selector) =>
            selector.Body is MemberExpression { Member: PropertyInfo property }
                ? property
                : throw new ArgumentException("selector expression must be a member expression");

        internal void SetFormatter(Type type, PropertyInfo? property, Func<object, string> formatter)
        {
            if (property == null) formattersByType[type] = formatter;
            else formattersByProperty[property] = formatter;
        }
    }
}