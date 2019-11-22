using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public interface IPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> AddCustomSerialization(Type type, Delegate func);
        PrintingConfig<TOwner> AddCustomPropertySerialization(string propertyName, Delegate func);
        PrintingConfig<TOwner> SetTypeCulture(Type type, CultureInfo culture);
        PrintingConfig<TOwner> TrimmedToLength(int maxLen);
        PrintingConfig<TOwner> TrimmedToLength(string propertyName, int maxLen);
    }

    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly TOwner value;

        private readonly Dictionary<Type, Delegate> customSerializations =
            new Dictionary<Type, Delegate>();

        private readonly Dictionary<Type, CultureInfo> typeCultures = new Dictionary<Type, CultureInfo>();

        private readonly Dictionary<string, Delegate> customPropertySerializations =
            new Dictionary<string, Delegate>();
        
        private readonly Dictionary<string, int> stringPropertiesToTrim = new Dictionary<string, int>();

        private readonly List<Type> excludedTypes = new List<Type>();

        private bool trimEnabled = false;
        private int maxLen = 0;

        public PrintingConfig(TOwner value)
        {
            this.value = value;
        }

        public PrintingConfig()
        {
            value = default;
        }

        public string PrintToString()
        {
            return PrintToString(value);
        }

        public string PrintToString(int nestingLevel)
        {
            return PrintToString(value, nestingLevel);
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;
            
            var type = obj.GetType();
            if (customSerializations.ContainsKey(type))
            {
                var serializer = customSerializations[type];
                return serializer.DynamicInvoke(obj) + Environment.NewLine;
            }

            var finalTypes = new[]
            {
                typeof(float), typeof(double), typeof(decimal),
                typeof(sbyte), typeof(byte),
                typeof(short), typeof(ushort),
                typeof(int), typeof(uint),
                typeof(long), typeof(ulong),
                typeof(string), typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(type))
            {
                if (!typeCultures.ContainsKey(type)) return obj + Environment.NewLine;
                var culture = typeCultures[type];
                dynamic number = Convert.ChangeType(obj, type);
                return number.ToString(culture) + Environment.NewLine;
            }

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedTypes.Contains(propertyInfo.PropertyType))
                    continue;
                
                var objValue = propertyInfo.GetValue(obj);

                if (propertyInfo.PropertyType == typeof(string))
                {
                    objValue = TrimToLengthIfNeeded(propertyInfo.Name, objValue as string);
                }

                if (customPropertySerializations.ContainsKey(propertyInfo.Name))
                    sb.Append(indentation + propertyInfo.Name + " = " +
                              customPropertySerializations[propertyInfo.Name]
                                  .DynamicInvoke(objValue) + Environment.NewLine);
                else
                    sb.Append(indentation + propertyInfo.Name + " = " +
                              PrintToString(objValue,
                                  nestingLevel + 1));
            }

            return sb.ToString();
        }

        private string TrimToLengthIfNeeded(string propName, string str)
        {
            if (trimEnabled)
            {
                return str.Substring(0, maxLen);
            }

            if (stringPropertiesToTrim.ContainsKey(propName))
            {
                return str.Substring(0, stringPropertiesToTrim[propName]);
            }

            return str;
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            if (!excludedTypes.Contains(typeof(T)))
                excludedTypes.Add(typeof(T));
            return this;
        }

        public PropertyPrintingConfig<TOwner, T> Printing<T>(Expression<Func<TOwner, T>> expression)
        {
            var propertyInfo = ((MemberExpression) expression.Body).Member as PropertyInfo;
            return new PropertyPrintingConfig<TOwner, T>(this, propertyInfo);
        }

        public PropertyPrintingConfig<TOwner, T> Printing<T>()
        {
            return new PropertyPrintingConfig<TOwner, T>(this);
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> expression)
        {
            return this;
        }

        PrintingConfig<TOwner> IPrintingConfig<TOwner>.AddCustomSerialization(Type type, Delegate func)
        {
            customSerializations[type] = func;
            return this;
        }

        PrintingConfig<TOwner> IPrintingConfig<TOwner>.SetTypeCulture(Type type, CultureInfo culture)
        {
            typeCultures[type] = culture;
            return this;
        }

        PrintingConfig<TOwner> IPrintingConfig<TOwner>.AddCustomPropertySerialization(string propertyName,
            Delegate func)
        {
            customPropertySerializations[propertyName] = func;
            return this;
        }

        PrintingConfig<TOwner> IPrintingConfig<TOwner>.TrimmedToLength(int maxLen)
        {
            trimEnabled = true;
            this.maxLen = maxLen;
            return this;
        }
        
        PrintingConfig<TOwner> IPrintingConfig<TOwner>.TrimmedToLength(string propertyName, int maxLen)
        {
            stringPropertiesToTrim[propertyName] = maxLen;
            return this;
        }
    }
}