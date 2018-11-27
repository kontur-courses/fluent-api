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
        private readonly Type[] finalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<string> excludedProperties = new HashSet<string>();
        private readonly Dictionary<Type, Delegate> typeSerializations = new Dictionary<Type, Delegate>();
        private readonly Dictionary<string, Delegate> propertySerializations = new Dictionary<string, Delegate>();
        private readonly Dictionary<Type, CultureInfo> numberCultures = new Dictionary<Type, CultureInfo>();

        internal void AddTypeSerialization<TPropType>(Delegate func)
        {
            typeSerializations[typeof(TPropType)] = func;
        }

        internal void AddPropertySerialization<TPropType>(string propName, Delegate func)
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
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in GetProperties(type))
            {
                string strValue;

                if (typeSerializations.ContainsKey(propertyInfo.PropertyType)
                ) // TODO Рефактор и вынести в отедльные методы
                {
                    var tDelegate = typeSerializations[propertyInfo.PropertyType];
                    strValue = (string) tDelegate.DynamicInvoke(propertyInfo.GetValue(obj));
                }
                else if (numberCultures.ContainsKey(propertyInfo.PropertyType))
                {
                    strValue = ((IFormattable) propertyInfo.GetValue(obj)).ToString(null,
                        numberCultures[propertyInfo.PropertyType]);
                }
                else if (propertySerializations.ContainsKey(propertyInfo.Name))
                {
                    var tDelegate = propertySerializations[propertyInfo.Name];
                    strValue = (string) tDelegate.DynamicInvoke(propertyInfo.GetValue(obj));
                }
                else
                {
                    strValue = PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
                }

                sb.Append(identation + propertyInfo.Name + " = " + strValue);
            }

            return sb.ToString();
        }

        private IEnumerable<PropertyInfo> GetProperties(Type objType)
        {
            foreach (var propertyInfo in objType.GetProperties())
            {
                if (excludedTypes.Contains(propertyInfo.PropertyType))
                    continue;
                if (excludedProperties.Contains(propertyInfo.Name))
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
            return new PropertyPrintingConfig<TOwner, TPropType>(this,
                ((MemberExpression) propertyFunc.Body).Member.Name);
        }

        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> propertyFunc)
        {
            excludedProperties.Add(((MemberExpression) propertyFunc.Body).Member.Name);
            return this;
        }
    }
}