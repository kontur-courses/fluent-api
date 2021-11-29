using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Printer
{
    public class PrintingConfig<TOwner>
    {
        private readonly Dictionary<Type, Func<object, string>> customSerializers = new();
        private readonly List<Type> excludedTypes = new();
        private CultureInfo commonFormatProvider = CultureInfo.InvariantCulture;
        private readonly Dictionary<Type, (CultureInfo formatProvider, string format)> formatProviders = new();

        private static readonly Type[] FinalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid), null
        };

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel, int recursionLimit = 50)
        {
            if (nestingLevel >= recursionLimit)
            {
                return string.Empty;
            }

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();

            sb.AppendLine(type.Name);
            if (customSerializers.ContainsKey(type))
            {
                sb.Append(identation + customSerializers[type](obj));
                return sb.ToString();
            }

            if (FinalTypes.Contains(obj.GetType()))
            {
                return Serialize(obj);
            }

            foreach (var propertyInfo in type.GetProperties().Where(x => !excludedTypes.Contains(x.PropertyType)))
            {
                sb.Append(identation + Serialize(propertyInfo, obj, nestingLevel));
            }

            return sb.ToString();
        }

        public PrintingConfig<TOwner> Exclude<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> StringOf<T>(Func<T, string> serializer)
        {
            customSerializers[typeof(T)] = e => serializer((T)e);
            return this;
        }

        public PrintingConfig<TOwner> WithCultureFor<T>(CultureInfo formatProvider = null, string format = null)
            where T : IFormattable
        {
            formatProviders[typeof(T)] = (formatProvider, format);
            return this;
        }

        public PrintingConfig<TOwner> WithCulture(CultureInfo formatProvider)
        {
            commonFormatProvider = formatProvider;
            return this;
        }

        private string Serialize(PropertyInfo propertyInfo, object obj, int nestingLevel)
        {
            if (customSerializers.ContainsKey(propertyInfo.PropertyType))
            {
                return propertyInfo.Name + " = " +
                       customSerializers[propertyInfo.PropertyType](propertyInfo.GetValue(obj)) + Environment.NewLine;
            }

            return propertyInfo.Name + " = " + PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
        }

        private string Serialize(object obj)
        {
            if (obj is not IFormattable formattable)
            {
                return obj + Environment.NewLine;
            }

            var type = formattable.GetType();
            if (formatProviders.ContainsKey(type))
            {
                return formattable.ToString(formatProviders[type].format, formatProviders[type].formatProvider)
                       + Environment.NewLine;
            }

            return formattable.ToString(null, commonFormatProvider) + Environment.NewLine;
        }
    }
}