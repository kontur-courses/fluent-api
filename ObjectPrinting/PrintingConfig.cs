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
        PrintingConfig<TOwner> SetTypeCulture(Type type, CultureInfo culture);
    }

    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly TOwner value;

        private readonly Dictionary<Type, Delegate> customSerializations =
            new Dictionary<Type, Delegate>();

        private readonly Dictionary<Type, CultureInfo> typeCultures = new Dictionary<Type, CultureInfo>();

        private readonly List<Type> excludedTypes = new List<Type>();

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

            var finalTypes = new[]
            {
                typeof(float), typeof(double), typeof(decimal),
                typeof(sbyte), typeof(byte),
                typeof(short), typeof(ushort),
                typeof(int), typeof(uint),
                typeof(long), typeof(ulong),
                typeof(string), typeof(DateTime), typeof(TimeSpan)
            };
            var objType = obj.GetType();
            if (finalTypes.Contains(objType))
            {
                if (!typeCultures.ContainsKey(objType)) return obj + Environment.NewLine;
                var culture = typeCultures[objType];
                dynamic number = Convert.ChangeType(obj, objType);
                return number.ToString(culture) + Environment.NewLine;
            }

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = objType;

            if (customSerializations.ContainsKey(type))
            {
                var serializer = customSerializations[type];
                sb.Append(serializer.DynamicInvoke(obj));
                sb.Append(Environment.NewLine);
            }
            else
            {
                sb.AppendLine(type.Name);
                foreach (var propertyInfo in type.GetProperties())
                {
                    if (excludedTypes.Contains(propertyInfo.PropertyType))
                        continue;
                    sb.Append(indentation + propertyInfo.Name + " = " +
                              PrintToString(propertyInfo.GetValue(obj),
                                  nestingLevel + 1));
                }
            }

            return sb.ToString();
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            if (!excludedTypes.Contains(typeof(T)))
                excludedTypes.Add(typeof(T));
            return this;
        }

        public PropertyPrintingConfig<TOwner, T> Printing<T>(Func<TOwner, T> func)
        {
            return new PropertyPrintingConfig<TOwner, T>(this);
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
    }
}