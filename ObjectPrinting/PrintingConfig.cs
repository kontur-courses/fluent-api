using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> mutedTypes = new HashSet<Type>();
        private readonly HashSet<string> mutedProperties = new HashSet<string>();
        private readonly Dictionary<Type, Delegate> alternativeTypeSerializers = new Dictionary<Type, Delegate>();
        private readonly Dictionary<string, Delegate> alternativePropertySerializers = new Dictionary<string, Delegate>();
        private readonly Dictionary<Type, CultureInfo> cultures = new Dictionary<Type, CultureInfo>();

        private static int maxNestingLevel = 5;
        private static Type[] finalTypes = {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        public void AddAlternativeTypeSerializer<TProperty>(Func<TProperty, string> printingFunc)
        {
            alternativeTypeSerializers[typeof(TProperty)] = printingFunc;
        }

        public void AddAlternativePropertySerializer<TProperty>(Func<TProperty, string> printingFunc, string propertyName)
        {
            alternativePropertySerializers[propertyName] = printingFunc;
        }

        public void AddCultureInfo(Type type, CultureInfo culture)
        {
            cultures[type] = culture;
        }

        public PrintingConfig<TOwner> Excluding<TProperty>()
        {
            mutedTypes.Add(typeof(TProperty));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TProperty>(Expression<Func<TOwner, TProperty>> memberSelector)
        {
            var propertyInfo = (PropertyInfo)((MemberExpression)memberSelector.Body).Member;
            var propertyName = propertyInfo.Name;
            mutedProperties.Add(propertyName);
            return this;
        }

        public PropertyPrintingConfig<TOwner, TProperty> Printing<TProperty>()
        {
            return new PropertyPrintingConfig<TOwner, TProperty>(this);
        }

        public PropertyPrintingConfig<TOwner, TProperty> Printing<TProperty>(Expression<Func<TOwner, TProperty>> memberSelector)
        {
            var propertyInfo = (PropertyInfo)((MemberExpression)memberSelector.Body).Member;
            var propertyName = propertyInfo.Name;
            return new PropertyPrintingConfig<TOwner, TProperty>(this, propertyName);
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public string PrintToString(IEnumerable<TOwner> collection)
        {
            var sb = new StringBuilder();
            sb.AppendLine(collection.GetType().Name);
            foreach (var item in collection)
            {
                var itemRepresentation = PrintToString(item, 1);
                sb.Append('\t');
                sb.Append(itemRepresentation);
            }
            return sb.ToString();
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;
            if (nestingLevel > maxNestingLevel)
                return "...";

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                var propertyType = propertyInfo.PropertyType;
                var propertyName = propertyInfo.Name;
                var propertyValueForObj = propertyInfo.GetValue(obj);

                if (mutedTypes.Contains(propertyType) || mutedProperties.Contains(propertyName)) continue;

                var valueRepresentation = GetSpecialRepresentationFor(propertyInfo, obj);
                if (valueRepresentation == null)
                {
                    sb.Append(indentation + propertyName + " = " +
                          PrintToString(propertyValueForObj, nestingLevel + 1));
                }
                else
                {
                    sb.Append(indentation)
                        .Append(propertyName).Append(" = ").Append(valueRepresentation)
                        .Append(Environment.NewLine);
                }
            }
            return sb.ToString();
        }

        private object GetSpecialRepresentationFor(PropertyInfo propertyInfo, object obj)
        {
            var propertyType = propertyInfo.PropertyType;
            var propertyName = propertyInfo.Name;
            var propertyValueForObj = propertyInfo.GetValue(obj);

            if (alternativeTypeSerializers.ContainsKey(propertyType))
                return alternativeTypeSerializers[propertyType].DynamicInvoke(propertyValueForObj);
            if (alternativePropertySerializers.ContainsKey(propertyName))
                return alternativePropertySerializers[propertyName].DynamicInvoke(propertyValueForObj);
            if (cultures.ContainsKey(propertyType))
                return ((IFormattable)propertyValueForObj).ToString(null, cultures[propertyType]);
            return null;
        }
    }
}