using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml.Schema;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private Dictionary<Type, Delegate> specialTypeSerialization = new Dictionary<Type, Delegate>();
        private Dictionary<Type, CultureInfo> cultureTypeSerialization = new Dictionary<Type, CultureInfo>();
        private HashSet<string> excludedProperties = new HashSet<string>();
        public int LengthOfStringProperties { get; set; } = -1;


        public void AddTypeSerializing(Type type, Delegate func)
        {
            specialTypeSerialization[type] = func;
        }
        
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyMapper = new PropertyMapper<TOwner>();
            var name = propertyMapper.PropertyName(memberSelector);
            excludedProperties.Add(name);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }
        
        
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var type = obj.GetType();
            var stringBuilder = new StringBuilder(type.Name + Environment.NewLine);
            foreach (var propertyInfo in type.GetProperties())
            {
                var propertyType = propertyInfo.PropertyType;
                var propertyName = propertyInfo.Name;
                if (excludedTypes.Contains(propertyType)) continue;
                if (excludedProperties.Contains(propertyName)) continue;
                stringBuilder.Append(identation + propertyInfo.Name + " = " +
                     PrintToString(GetPropertyObj(obj, propertyInfo), nestingLevel + 1));
            }
            return stringBuilder.ToString();
        }
        
        private object GetPropertyObj(object obj, PropertyInfo propertyInfo)
        {
            var propertyType = propertyInfo.PropertyType;
            if (specialTypeSerialization.ContainsKey(propertyType))
                return specialTypeSerialization[propertyType].DynamicInvoke(propertyInfo.GetValue(obj));
            if (cultureTypeSerialization.ContainsKey(propertyType))
                return ((IFormattable) propertyInfo.GetValue(obj)).ToString("G", cultureTypeSerialization[propertyType]);
            if (propertyType == typeof(string) && LengthOfStringProperties >= 0)
                return ((string) propertyInfo.GetValue(obj)).Substring(0, LengthOfStringProperties);
            return propertyInfo.GetValue(obj);

        }

        public void AddCultureForType(Type type, CultureInfo cultureInfo)
        {
            cultureTypeSerialization[type] = cultureInfo;
        }
    }
}