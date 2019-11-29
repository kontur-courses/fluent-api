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
        private readonly HashSet<Type> excludingTypes = new HashSet<Type>();
        private readonly HashSet<PropertyInfo> excludingProperties = new HashSet<PropertyInfo>();
        private readonly HashSet<object> usedFields = new HashSet<object>();

        private readonly Dictionary<Type, Expression<Func<object, string>>> typeSerializingMethods =
            new Dictionary<Type, Expression<Func<object, string>>>();

        private readonly Dictionary<PropertyInfo, Expression<Func<object, string>>> propertySerializingMethods =
            new Dictionary<PropertyInfo, Expression<Func<object, string>>>();
        private CultureInfo numbersCulture = CultureInfo.CurrentCulture;

        Dictionary<Type, Expression<Func<object, string>>> IPrintingConfig<TOwner>.SerializingMethods =>
            typeSerializingMethods;

        Dictionary<PropertyInfo, Expression<Func<object, string>>> IPrintingConfig<TOwner>.PropertySerializingMethods =>
            propertySerializingMethods;

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            excludingTypes.Add(typeof(T));
            return this;
        }

        public PropertySerializingConfig<TOwner, T> Serializing<T>()
        {
            return new PropertySerializingConfig<TOwner, T>(this);
        }

        public PropertySerializingConfig<TOwner, T> Serializing<T>(Expression<Func<TOwner, T>> expression)
        {
            var propertyInfo = ((MemberExpression) expression.Body).Member as PropertyInfo;
            return new PropertySerializingConfig<TOwner, T>(this, propertyInfo);
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> expression)
        {
            excludingProperties.Add(((MemberExpression) expression.Body).Member as PropertyInfo);
            return this;
        }

        public PrintingConfig<TOwner> UsingNumbersCulture(CultureInfo cultureInfo)
        {
            numbersCulture = cultureInfo;
            return this;
        }

        private string GetStringFromProperty(object currentObject, PropertyInfo propertyInfo, int nestingLevel)
        {
            if (propertyInfo.GetValue(currentObject) is null)
                return "null";
            var value = propertyInfo.GetValue(currentObject);
            if (propertySerializingMethods.ContainsKey(propertyInfo))
                return propertySerializingMethods[propertyInfo].Compile()
                    .Invoke(value) + $" (Hash: {value.GetHashCode()})";

            if (typeSerializingMethods.ContainsKey(propertyInfo.PropertyType))
                return typeSerializingMethods[propertyInfo.PropertyType].Compile()
                    .Invoke(value) + $" (Hash: {value.GetHashCode()})";
            
            return PrintToString(value, nestingLevel);

        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null";

            var numberTypes = new[]
            {
                typeof(int), typeof(double), typeof(float),
            };
            if (numberTypes.Contains(obj.GetType()))
            {
                return $"{NumberToStringUsingCulture(obj)} (Hash: {obj.GetHashCode()})";
            }

            if (obj is DateTime || obj is TimeSpan || obj is string)
                return $"{obj} (Hash: {obj.GetHashCode()})";

            if (usedFields.Contains(obj))
                return $"<was above> (Hash: {obj.GetHashCode()})";
            usedFields.Add(obj);
            var result = "";
            if (obj is IEnumerable enumerable)
                result = GetIEnumerableString(enumerable, nestingLevel + 1) + $" (Hash: {obj.GetHashCode()})";
            else
                result = ComplexObjectToString(obj, nestingLevel);


            usedFields.Remove(obj);
            return result;
        }

        private string GetIEnumerableString(IEnumerable obj, int nestingLevel)
        {
            var builder = new StringBuilder("{").Append(Environment.NewLine);
            var counter = 0;
            foreach (var elem in obj)
            {
                builder.Append($"{new string('\t', nestingLevel)}{counter}: {PrintToString(elem, nestingLevel + 1)}{Environment.NewLine}");
                counter++;
            }

            return builder.Append("}").ToString();
        }

        private string NumberToStringUsingCulture(object obj)
        {
            switch (obj)
            {
                case int intObj:
                    return intObj.ToString(numbersCulture);
                case double objDouble:
                    return objDouble.ToString(numbersCulture);
                case float objFloat:
                    return objFloat.ToString(numbersCulture);
            }

            return obj.ToString();
        }

        private string ComplexObjectToString(object obj, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine($"{type.Name} (Hash: {obj.GetHashCode()})");

            foreach (var propertyInfo in type.GetProperties().Where(p =>
                !excludingTypes.Contains(p.PropertyType) && !excludingProperties.Contains(p)))
            {
                sb.Append(identation + propertyInfo.Name + " = " +
                          GetStringFromProperty(obj, propertyInfo, nestingLevel + 1) + Environment.NewLine);
            }

            return sb.ToString();
        }
    }
}