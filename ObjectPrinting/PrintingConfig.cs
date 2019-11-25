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
        private static readonly Type[] FinalTypes =
            {typeof(int), typeof(double), typeof(float), typeof(string), typeof(DateTime), typeof(TimeSpan)};

        private readonly HashSet<Type> excludedTypes;

        private readonly HashSet<PropertyInfo> excludedProperties;

        private readonly Dictionary<Type, Delegate> typeSerializers;

        private readonly Dictionary<PropertyInfo, Delegate> propertySerializers;

        private readonly Dictionary<Type, CultureInfo> typeCultures;

        private readonly  Dictionary<PropertyInfo, int> trimLengthDictionary;

        public PrintingConfig()
        {
            excludedTypes = new HashSet<Type>();
            typeSerializers = new Dictionary<Type, Delegate>();
            propertySerializers = new Dictionary<PropertyInfo, Delegate>();
            typeCultures = new Dictionary<Type, CultureInfo>();
            excludedProperties = new HashSet<PropertyInfo>();
            trimLengthDictionary = new Dictionary<PropertyInfo, int>();
        }

        Dictionary<Type, Delegate> IPrintingConfig<TOwner>.TypeSerializers => typeSerializers;

        Dictionary<PropertyInfo, Delegate> IPrintingConfig<TOwner>.PropertySerializers => propertySerializers;

        Dictionary<PropertyInfo, int> IPrintingConfig<TOwner>.TrimLengthDictionary => trimLengthDictionary;

        Dictionary<Type, CultureInfo> IPrintingConfig<TOwner>.TypeCultures => typeCultures;


        public TypePrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, GetPropertyFromExpression(memberSelector));
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludedProperties.Add(GetPropertyFromExpression(memberSelector));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0, new HashSet<object>());
        }

        private PropertyInfo GetPropertyFromExpression<TPropType>(Expression<Func<TOwner, TPropType>> expression)
        {
            if (!(expression.Body is MemberExpression memberExpression))
                throw new ArgumentException();

            return memberExpression.Member as PropertyInfo;
        }

        private string PrintToString(object obj, int nestingLevel, HashSet<object> printedObjects)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();

            if (FinalTypes.Contains(type))
                return obj + Environment.NewLine;

            if (type.IsGenericType)
                if (type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                {
                    var kvpProperties = type.GetProperties();
                    return $"{kvpProperties[0].GetValue(obj)} = {kvpProperties[1].GetValue(obj)}"
                           + Environment.NewLine;
                }

            if (printedObjects.Contains(obj))
                return $"(this) {type.Name}" + Environment.NewLine;

            printedObjects = new HashSet<object>(printedObjects) {obj};

            if (obj is IEnumerable collection) 
                return PrintEnumerableToString(collection, nestingLevel, printedObjects);

            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            AppendFromNestedTypes(sb, obj, nestingLevel, printedObjects);

            return sb.ToString();
        }

        private string PrintEnumerableToString(IEnumerable collection, int nestingLevel, HashSet<object> printedObjects)
        {
            var sb = new StringBuilder();
            var indentation = new string('\t', nestingLevel + 1);

            if (collection.GetEnumerator().MoveNext() == false)
            {
                sb.Append($"Empty {collection.GetType().Name}" + Environment.NewLine);
                return sb.ToString();
            }

            sb.Append(Environment.NewLine);
            foreach (var item in collection)
                sb.Append(indentation + PrintToString(item, nestingLevel + 1, printedObjects));

            return sb.ToString();
        }
        private void AppendFromNestedTypes(StringBuilder stringBuilder, object obj, int nestingLevel,
            HashSet<object> printedObjects)
        {
            var type = obj.GetType();
            var indentation = new string('\t', nestingLevel + 1);

            foreach (var propertyInfo in type.GetProperties())
                if (!excludedTypes.Contains(propertyInfo.PropertyType) && !excludedProperties.Contains(propertyInfo))
                {
                    stringBuilder.Append(indentation
                                         + propertyInfo.Name + " = "
                                         + SerializeFromProperty(
                                             propertyInfo,
                                             propertyInfo.GetValue(obj),
                                             nestingLevel + 1,
                                             printedObjects
                                             )
                                         );
                }
        }

        private string SerializeFromProperty(PropertyInfo propertyInfo, object obj, int nestingLevel,
            HashSet<object> printedObjects)
        {
            string result = null;
            if (typeCultures.ContainsKey(propertyInfo.PropertyType))
                result = string.Format(typeCultures[propertyInfo.PropertyType], "{0}", obj);

            if (typeSerializers.ContainsKey(propertyInfo.PropertyType))
                result = typeSerializers[obj.GetType()].DynamicInvoke(obj) as string;

            if (propertySerializers.ContainsKey(propertyInfo))
                result = propertySerializers[propertyInfo].DynamicInvoke(obj) as string;

            if (trimLengthDictionary.ContainsKey(propertyInfo))
            {
                if (result == null)
                    result = obj as string;

                result = result?.Substring(0, Math.Min(result.Length, trimLengthDictionary[propertyInfo]));
            }

            return result == null ? PrintToString(obj, nestingLevel, printedObjects) : result + Environment.NewLine;
        }
    }
}