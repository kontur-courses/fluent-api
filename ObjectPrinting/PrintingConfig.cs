using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private Dictionary<object, List<Func<object, object>>> configs = new Dictionary<object, List<Func<object, object>>>();

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
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                var propertyValue = propertyInfo.GetValue(obj);
                var hasTypeConfig = configs.TryGetValue(propertyInfo.PropertyType, out var typeConfig);
                if (hasTypeConfig)
                    propertyValue = ApplyConfigs(propertyValue, typeConfig);
                var hasNameConfig = configs.TryGetValue(propertyInfo.Name, out var nameConfig);
                if (hasNameConfig)
                    propertyValue = ApplyConfigs(propertyValue, nameConfig);
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyValue,
                              nestingLevel + 1));
            }
            return sb.ToString();
        }

        private object ApplyConfigs(object value, List<Func<object, object>> configs)
        {
            var result = value;
            foreach (var config in configs)
                result = config(result);
            return result;
        }

        public PrintingConfig<TOwner> Exclude<T>()
        {
            return new PrintingConfig<TOwner>();
        }

        public PrintingConfig<TOwner> Exclude(string propertyName)
        {
            return new PrintingConfig<TOwner>();
        }

        public SerializePrintingConfig<TOwner, string> Serialize(string propertyName)
        {
            return new SerializePrintingConfig<TOwner, string>();
        }

        public SerializePrintingConfig<TOwner, T> Serialize<T>()
        {
            return new SerializePrintingConfig<TOwner, T>();
        }

        public CulturePrintingConfig<TOwner> SetCulture(CultureInfo cultureInfo)
        {
            return new CulturePrintingConfig<TOwner>();
        }
    }
}