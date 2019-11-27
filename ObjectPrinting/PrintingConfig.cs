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
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };
        private readonly HashSet<object> propertiesInRecursion = new HashSet<object>();
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<PropertyInfo> excludedProperties = new HashSet<PropertyInfo>();
        private readonly Dictionary<PropertyInfo, int> maxLengthOfStringProperty =
            new Dictionary<PropertyInfo, int>();
        private readonly Dictionary<Type, Func<object, string>> exactSerializationForType =
            new Dictionary<Type, Func<object, string>>();
        private readonly Dictionary<PropertyInfo, Func<object, string>> exactSerializationForProperty =
            new Dictionary<PropertyInfo, Func<object, string>>();
        private readonly Dictionary<Type, CultureInfo> numberTypesCultures =
            new Dictionary<Type, CultureInfo>();

        //Это все сделано через интерфейс, чтобы скрыть от пользователя
        Dictionary<Type, Func<object, string>> IPrintingConfig<TOwner>.SerializationForType =>
            exactSerializationForType;
        Dictionary<PropertyInfo, Func<object, string>> IPrintingConfig<TOwner>.SerializationForProperty =>
            exactSerializationForProperty;
        Dictionary<Type, CultureInfo> IPrintingConfig<TOwner>.NumberTypesCultures => numberTypesCultures;
        Dictionary<PropertyInfo, int> IPrintingConfig<TOwner>.MaxLengthOfStringProperty
        {
            get => maxLengthOfStringProperty;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)   
                return "null" + Environment.NewLine;
            propertiesInRecursion.Add(obj);
            var type = obj.GetType();
            if (excludedTypes.Contains(type))
                return FinishWorkWithObjectAndReturnResult(obj, string.Empty);
            if (finalTypes.Contains(type))
                return FinishWorkWithObjectAndReturnResult(obj,
                    ReturnSerializationOFFinalTypeObject(obj));
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            if (IsEnumerable(obj))
            {
                foreach (var element in (IEnumerable) obj)
                    sb.Append(identation + PrintToString(element, nestingLevel + 1));
            }
            else
            {
                foreach (var propertyInfo in type.GetProperties())
                {
                    if (excludedProperties.Contains(propertyInfo))
                        continue;
                    if (excludedTypes.Contains(propertyInfo.PropertyType))
                        continue;
                    var objPropertyValue = new object();
                    objPropertyValue = propertyInfo.GetValue(obj);
                    if (propertiesInRecursion.Contains(objPropertyValue))
                        continue;
                    sb.Append(HandleOneProperty(propertyInfo, objPropertyValue, identation, nestingLevel));
                }
            }
            return FinishWorkWithObjectAndReturnResult(obj, sb.ToString());
        }

        private bool IsEnumerable(object obj) => obj is IEnumerable;

        private string FinishWorkWithObjectAndReturnResult(object obj, string result)
        {
            propertiesInRecursion.Remove(obj);
            return result;
        }

        private string HandleOneProperty(PropertyInfo propertyInfo, object objPropertyValue,
            string identation, int nestingLevel)
        {
            if (propertyInfo.PropertyType == typeof(string))
                objPropertyValue = GetShortenedStringFromObject(propertyInfo, objPropertyValue);
            if (exactSerializationForProperty.ContainsKey(propertyInfo))
                return exactSerializationForProperty[propertyInfo](
                    objPropertyValue);
            else if (exactSerializationForType.ContainsKey(propertyInfo.PropertyType))
                return exactSerializationForType[propertyInfo.PropertyType]
                    (objPropertyValue);
            else
                return identation + propertyInfo.Name + " = " +
                       PrintToString(objPropertyValue, nestingLevel + 1);
        }

        private string GetShortenedStringFromObject(PropertyInfo propertyInfo, object obj)
        {
            var maxLenForProperty = int.MaxValue;
            if (maxLengthOfStringProperty.ContainsKey(propertyInfo))
                maxLenForProperty = maxLengthOfStringProperty[propertyInfo];
            return obj.ToString().Substring(0, Math.Min(maxLenForProperty, obj.ToString().Length));
        } 


        private string ReturnSerializationOFFinalTypeObject(object obj)
        {
            var objToSerialize = obj;
            var objType = objToSerialize.GetType();
            if (exactSerializationForType.ContainsKey(objType))
                return exactSerializationForType[objType](objToSerialize)
                       + Environment.NewLine;
            if (numberTypesCultures.ContainsKey(objType)) //Now it's only for int
                return int.Parse(objToSerialize.ToString(),
                           numberTypesCultures[objType]) + Environment.NewLine;
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