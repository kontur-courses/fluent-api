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
    public class PrintingConfig<TOwner> :
        IPrintingConfig<TOwner>
    {
        private static readonly Type[] finalTypes = new Type[]
        {
            typeof(int), typeof(short), typeof(long), typeof(double), typeof(float),
            typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(bool),
            typeof(byte), typeof(decimal), typeof(ushort), typeof(ulong),
            typeof(uint), typeof(sbyte)
        };
        private readonly HashSet<object> objectsInCurrentGraph = new HashSet<object>();
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<PropertyInfo> excludedProperties = new HashSet<PropertyInfo>();
        private readonly Dictionary<PropertyInfo, int> maxLengthOfStringProperty =
            new Dictionary<PropertyInfo, int>();
        private readonly Dictionary<Type, Func<object, string>> exactSerializationForType =
            new Dictionary<Type, Func<object, string>>();
        private readonly Dictionary<PropertyInfo, Func<object, string>> exactSerializationForProperty =
            new Dictionary<PropertyInfo, Func<object, string>>();
        private CultureInfo culture = CultureInfo.CurrentCulture;

        //All of this is done with interfaces to hide this fields from user
        Dictionary<Type, Func<object, string>> IPrintingConfig<TOwner>.SerializationForType =>
            exactSerializationForType;
        Dictionary<PropertyInfo, Func<object, string>> IPrintingConfig<TOwner>.SerializationForProperty =>
            exactSerializationForProperty;

        Dictionary<PropertyInfo, int> IPrintingConfig<TOwner>.MaxLengthOfStringProperty => maxLengthOfStringProperty;

        public string PrintToString(TOwner obj)
        {
            return GetSerializationOfObject(obj, 0);
        }

        private string GetSerializationOfObject(object obj, int nestingLevel)
        {
            if (obj == null)   
                return "null" + Environment.NewLine;

            objectsInCurrentGraph.Add(obj);

            var type = obj.GetType();
            if (excludedTypes.Contains(type))
            {
                objectsInCurrentGraph.Remove(obj);
                return string.Empty;
            }

            if (finalTypes.Contains(type))
            {
                objectsInCurrentGraph.Remove(obj);
                return ReturnSerializationOfObjectOfFinalType(obj);
            }

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            if (obj is IEnumerable)
            {
                foreach (var element in (IEnumerable)obj)
                    sb.Append(indentation + GetSerializationOfObject(element, nestingLevel + 1));
            }
            else
                sb.Append(GetSerializationOfPropertiesAndFieldsOfObject(obj, indentation, nestingLevel));
            objectsInCurrentGraph.Remove(obj);
            return sb.ToString();
        }

        private string GetSerializationOfPropertiesAndFieldsOfObject(object obj, string indentation, int nestingLevel)
        {
            var sb = new StringBuilder();
            var type = obj.GetType();
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedProperties.Contains(propertyInfo))
                    continue;
                if (excludedTypes.Contains(propertyInfo.PropertyType))
                    continue;
                var objPropertyValue = propertyInfo.GetValue(obj);
                if (objectsInCurrentGraph.Contains(objPropertyValue))
                {
                    sb.Append(indentation + propertyInfo.Name + " = " + "<cyclic link is detected>" +
                              Environment.NewLine);
                    continue;
                }

                sb.Append(HandleOneProperty(propertyInfo, objPropertyValue, indentation, nestingLevel));
            }

            foreach (var fieldInfo in type.GetFields())
                sb.Append(indentation + fieldInfo.Name + " = " +
                          GetSerializationOfObject(fieldInfo.GetValue(obj), nestingLevel + 1));

            return sb.ToString();
        }

        private string HandleOneProperty(PropertyInfo propertyInfo, object objPropertyValue, string indentation, int nestingLevel)
        {
            if (propertyInfo.PropertyType == typeof(string))
                objPropertyValue = GetShortenedStringFromObject(propertyInfo, objPropertyValue);

            if (exactSerializationForProperty.ContainsKey(propertyInfo))
                return exactSerializationForProperty[propertyInfo](objPropertyValue);

            if (exactSerializationForType.ContainsKey(propertyInfo.PropertyType))
                return exactSerializationForType[propertyInfo.PropertyType](objPropertyValue);

            return indentation + propertyInfo.Name + " = " +
                   GetSerializationOfObject(objPropertyValue, nestingLevel + 1);
        }

        private string GetShortenedStringFromObject(PropertyInfo propertyInfo, object obj)
        {
            var maxLenForProperty = int.MaxValue;
            if (maxLengthOfStringProperty.ContainsKey(propertyInfo))
                maxLenForProperty = maxLengthOfStringProperty[propertyInfo];
            return obj.ToString().Substring(0, Math.Min(maxLenForProperty, obj.ToString().Length));
        } 


        private string ReturnSerializationOfObjectOfFinalType(object obj)
        {
            var objToSerialize = obj;
            var objType = objToSerialize.GetType();

            if (exactSerializationForType.ContainsKey(objType))
                return exactSerializationForType[objType](objToSerialize) + Environment.NewLine;

            if (objToSerialize is IFormattable)
                return (objToSerialize as IFormattable).ToString(null, culture) + Environment.NewLine;

            return objToSerialize + Environment.NewLine;
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> func)
        {
            excludedProperties.Add(((MemberExpression)func.Body).Member as PropertyInfo);
            return this;
        }

        public PrintingConfig<TOwner> HavingCulture(CultureInfo culture)
        {
            this.culture = culture;
            return this;
        }

        public PropertySerializingConfig<TOwner, T> Serializing<T>()
        {
            return new PropertySerializingConfig<TOwner, T>(this);
        }

        public PropertySerializingConfig<TOwner, T> Serializing<T>(Expression<Func<TOwner, T>> func)
        {
            var propertyInfo = ((MemberExpression)func.Body).Member as PropertyInfo;
            return new PropertySerializingConfig<TOwner, T>(this, propertyInfo);
        }
    }
}