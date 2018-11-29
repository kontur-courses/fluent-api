using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly Stack<object> currentObjects = new Stack<object>();
        private static readonly Type[] finalTypes = {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<(Type, string)> excludedProperties = new HashSet<(Type, string)>();
        private readonly Dictionary<Type, Func<object, string>> typeSerializationSettings =
            new Dictionary<Type, Func<object, string>>();
        private readonly Dictionary<(Type, string), Func<object, string>> propertySerializationSettings = 
            new Dictionary<(Type, string), Func<object, string>>();
        private readonly Dictionary<(Type, string), int> propertyTrimmingSettings = new Dictionary<(Type, string), int>();
        private readonly Dictionary<Type, CultureInfo> cultureInfoSettings = 
            new Dictionary<Type, CultureInfo>();

        Dictionary<Type, Func<object, string>> IPrintingConfig<TOwner>.TypeSerializationSettings => typeSerializationSettings;
        Dictionary<Type, CultureInfo> IPrintingConfig<TOwner>.CultureInfoSettings => cultureInfoSettings;
        Dictionary<(Type, string), Func<object, string>> IPrintingConfig<TOwner>.PropertySerializationSettings =>
            propertySerializationSettings;
        Dictionary<(Type, string), int> IPrintingConfig<TOwner>.PropertyTrimmingSettings => propertyTrimmingSettings;

        public PrintingConfig<TOwner> ExcludingType<TType>()
        {
            excludedTypes.Add(typeof(TType));
            return this;
        }

        public PrintingConfig<TOwner> ExcludingProperty<TType>(Expression<Func<TOwner, TType>> propertySelector)
        {
            var propertyExpression = (MemberExpression)propertySelector.Body;
            excludedProperties.Add((propertyExpression.Member.DeclaringType, propertyExpression.Member.Name));
            return this;
        }

        public TypeSerializingContext<TOwner, TType> Serializing<TType>()
        {
            return new TypeSerializingContext<TOwner, TType>(this);
        }

        public PropertySerializingContext<TOwner, TType> Serializing<TType>(Expression<Func<TOwner, TType>> propertySelector)
        {
            var propertyExpression = (MemberExpression) propertySelector.Body;
            return new PropertySerializingContext<TOwner, TType>(this,
                (propertyExpression.Member.DeclaringType, propertyExpression.Member.Name));
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (TryGetFinalObjectStringValue(obj, out var finalValue))
                return finalValue;
            currentObjects.Push(obj);
            var type = obj.GetType();
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            if (obj is IEnumerable collection)
                sb.Append(PrintElements(collection, nestingLevel));
            else
                sb.Append(PrintProperties(obj, nestingLevel));
            currentObjects.Pop();
            return sb.ToString();
        }

        private string PrintProperties(object obj, int nestingLevel)
        {
            var type = obj.GetType();
            var sb = new StringBuilder();
            var indentation = new string('\t', nestingLevel + 1);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (!TryGetPropertyStringValue(obj, propertyInfo, nestingLevel, out var propertyRepresentation))
                    continue;
                sb.Append(indentation + propertyInfo.Name + " = " + propertyRepresentation);
            }

            return sb.ToString();
        }

        private string PrintElements(IEnumerable collection, int nestingLevel)
        {
            var sb = new StringBuilder();
            var indentation = new string('\t', nestingLevel + 1);
            var counter = 0;
            foreach (var element in collection)
            {
                var value = PrintToString(element, nestingLevel + 1);
                sb.Append(indentation + $"{counter++}: " + value);
            }

            return sb.ToString();
        }

        private bool TryGetFinalObjectStringValue(object obj, out string value)
        {
            var result = true;
            if (obj == null)
            { 
                value = "null" + Environment.NewLine;
                return true;
            }
            var type = obj.GetType();
            if (cultureInfoSettings.ContainsKey(type))
                value = ((IFormattable)obj).ToString(null, cultureInfoSettings[type]);
            else if (currentObjects.Contains(obj))
                value = type.Name + " (already printed)";
            else if (typeSerializationSettings.ContainsKey(type))
                value = typeSerializationSettings[type].Invoke(obj);
            else if (obj is IFormattable formattableObj)
                value = formattableObj.ToString(null, CultureInfo.InvariantCulture);
            else if (finalTypes.Contains(type))
                value = obj.ToString();
            else
            {
                result = false;
                value = null;
            }

            value += Environment.NewLine;
            return result;
        }

        private bool TryGetPropertyStringValue(object obj, PropertyInfo propertyInfo, int nestingLevel, out string value)
        {
            var type = obj.GetType();
            var property = (type, propertyInfo.Name);
            if (excludedTypes.Contains(propertyInfo.PropertyType) || excludedProperties.Contains(property))
            {
                value = null;
                return false;
            }
            value = propertySerializationSettings.ContainsKey(property) ?
                propertySerializationSettings[property].Invoke(propertyInfo.GetValue(obj)) + Environment.NewLine
                : PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);

            if (propertyTrimmingSettings.ContainsKey(property))
                value = value.Remove(propertyTrimmingSettings[property]) +
                                         Environment.NewLine;
            return true;
        }
    }
}