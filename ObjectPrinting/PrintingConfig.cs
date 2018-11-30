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
    public class PrintingConfig<TOwner>
    {
        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<PropertyInfo> excludedProperties = new HashSet<PropertyInfo>();
        private readonly Dictionary<Type, Func<object, string>> typeSerializations = new Dictionary<Type, Func<object, string>>();
        private readonly Dictionary<PropertyInfo, Func<object, string>> propertySerializations = new Dictionary<PropertyInfo, Func<object, string>>();
        private readonly Dictionary<Type, CultureInfo> numberCultures = new Dictionary<Type, CultureInfo>();
        
        internal void AddTypeSerialization<TPropType>(Func<object, string> func)
        {
            typeSerializations[typeof(TPropType)] = func;
        }

        internal void AddPropertySerialization<TPropType>(PropertyInfo propName, Func<object, string> func)
        {
            propertySerializations[propName] = func;
        }
        
        internal void SetNumberCulture<TPropType>(CultureInfo culture)
        {
            numberCultures[typeof(TPropType)] = culture;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }
        
        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();

            if (type is IEnumerable<object> colletion)
            {
                return PrintIEnumerableObject(colletion, nestingLevel + 1);
            }
            
            sb.AppendLine(type.Name);

            var props = GetProperties(type).ToList();
            if (props.Count == 0)
            {
                sb.AppendLine(Environment.NewLine + type.Name + " = " + obj.ToString());
                return sb.ToString();
            }
            
            foreach (var prop in props)
            {
                var customSerialization = TryGetCustomSerialization(prop, nestingLevel + 1);

                if (customSerialization == null)
                {
                    sb.Append(identation + prop.Name + " = " + PrintToString(prop.GetValue(obj), nestingLevel + 1));
                }
                else
                {
                    sb.Append(identation + prop.Name + " = " + customSerialization);
                }
            }

            return sb.ToString();
        }

        private string PrintIEnumerableObject(IEnumerable<object> obj, int nestingLevel)
        {
            var resultStr = new StringBuilder();
            var identation = new string('\t', nestingLevel + 1);
            
            foreach (var element in obj)
            {
                resultStr.Append(identation + PrintToString(element, nestingLevel + 1));
            }
            return resultStr.ToString();
        }
        
        
        private string TryGetCustomSerialization(PropertyInfo propertyInfo, object obj)
        {
            if (propertySerializations.ContainsKey(propertyInfo))
                return (string) propertySerializations[propertyInfo].DynamicInvoke(propertyInfo.GetValue(obj));
            
            if (numberCultures.ContainsKey(propertyInfo.PropertyType))
                return ((IFormattable) propertyInfo.GetValue(obj)).ToString(null,
                    numberCultures[propertyInfo.PropertyType]);

            if (typeSerializations.ContainsKey(propertyInfo.PropertyType))
                return (string) typeSerializations[propertyInfo.PropertyType].DynamicInvoke(propertyInfo.GetValue(obj));
            
            return null;
        }

        private IEnumerable<PropertyInfo> GetProperties(Type objType)
        {
            var x = objType.GetProperties();
            var y = objType.GetFields();
            
            foreach (var propertyInfo in objType.GetProperties())
            {
                if (excludedTypes.Contains(propertyInfo.PropertyType))
                    continue;
                if (excludedProperties.Contains(propertyInfo))
                    continue;
                yield return propertyInfo;
            }
        }

        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public TypePrintingConfig<TOwner, TPropType> SetAltSerialize<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> SetAltSerialize<TPropType>(
            Expression<Func<TOwner, TPropType>> propertyFunc)
        {
            var property = typeof(TOwner).GetProperty(((MemberExpression) propertyFunc.Body).Member.Name);
            return new PropertyPrintingConfig<TOwner, TPropType>(this, property);
        }

        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> propertyFunc)
        {
            var property = typeof(TOwner).GetProperty(((MemberExpression) propertyFunc.Body).Member.Name);
            excludedProperties.Add(property);
            return this;
        }
    }
}