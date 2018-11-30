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
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        private readonly Type[] numeric =
        {
            typeof(int), typeof(double), typeof(float), typeof(decimal),
            typeof(byte), typeof(long), typeof(short)
        };
        
        

        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<PropertyInfo> excludedProperties = new HashSet<PropertyInfo>();

        private readonly Dictionary<Type, Func<object, string>> typeSerializations =
            new Dictionary<Type, Func<object, string>>();

        private readonly Dictionary<PropertyInfo, Func<object, string>> propertySerializations =
            new Dictionary<PropertyInfo, Func<object, string>>();

        private readonly Dictionary<PropertyInfo, int> propertyTrimmCuts = new Dictionary<PropertyInfo, int>();
        private readonly Dictionary<Type, CultureInfo> numberCultures = new Dictionary<Type, CultureInfo>();

        internal void AddTypeSerialization<TPropType>(Func<object, string> func)
        {
            typeSerializations[typeof(TPropType)] = func;
        }

        internal void AddPropertySerialization(PropertyInfo propName, Func<object, string> func)
        {
            propertySerializations[propName] = func;
        }

        internal void AddTrimmCut(PropertyInfo propName, int num)
        {
            propertyTrimmCuts[propName] = num;
        }

        internal void SetNumberCulture<TPropType>(CultureInfo culture)
        {
            if(!numeric.Contains(typeof(TPropType)))
                throw new ArgumentException("Wrond type of data. Type of value must be Numeric");
            
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

            if (finalTypes.Contains(obj.GetType()) || nestingLevel == 10)
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();

            if (type.GetInterface(nameof(ICollection)) != null) 
                return PrintIEnumerableObject((IEnumerable<object>)obj, nestingLevel);

            sb.AppendLine(type.Name);

            var propertyInfos = GetProperties(type).ToList();
            if (propertyInfos.Count == 0)
            {
                sb.AppendLine(Environment.NewLine + type.Name + " = " + obj);
                return sb.ToString();
            }

            foreach (var prop in propertyInfos)
            {
                var customSerialization = TryGetCustomSerialization(prop, obj);

                if (customSerialization == null)
                    sb.Append(identation + prop.Name + " = " + PrintToString(prop.GetValue(obj), nestingLevel + 1));
                else
                    sb.AppendLine(identation + prop.Name + " = " + customSerialization);
            }

            return sb.ToString();
        }

        private string PrintIEnumerableObject(IEnumerable<object> obj, int nestingLevel)
        {
            var resultStr = new StringBuilder();
            var identation = new string('\t', nestingLevel);

            foreach (var element in obj) resultStr.Append(identation + PrintToString(element, nestingLevel));

            return resultStr.ToString();
        }

        private string TryGetCustomSerialization(PropertyInfo propertyInfo, object obj)
        {
            var value = propertyInfo.GetValue(obj);
            var isCustomSer = false;

            if (propertySerializations.ContainsKey(propertyInfo))
            {
                value = (string) propertySerializations[propertyInfo].DynamicInvoke(value);
                isCustomSer = true;
            }


            if (numberCultures.ContainsKey(propertyInfo.PropertyType))
            {
                if (value is IFormattable)
                {
                    value = ((IFormattable) value).ToString(null, numberCultures[propertyInfo.PropertyType]);
                    isCustomSer = true;
                }
            }

            if (typeSerializations.ContainsKey(propertyInfo.PropertyType))
            {
                value = (string) typeSerializations[propertyInfo.PropertyType].DynamicInvoke(value);
                isCustomSer = true;
            }

            if (propertyTrimmCuts.ContainsKey(propertyInfo))
            {
                value = ((string) value).Substring(0, propertyTrimmCuts[propertyInfo]);
                isCustomSer = true;
            }

            return isCustomSer ? (string) value : null;
        }

        private IEnumerable<PropertyInfo> GetProperties(Type objType)
        {
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