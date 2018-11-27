using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        //public readonly ImmutableDictionary<object, List<Func<object, string>>> configs 
        //    = ImmutableDictionary<object, List<Func<object, string>>>.Empty;

        public readonly ImmutableDictionary<object, List<Delegate>> configs 
            = ImmutableDictionary<object, List<Delegate>>.Empty;

        public PrintingConfig(ImmutableDictionary<object, List<Func<object, string>>> configs)
        {
            this.configs = configs;
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
                if (propertyValue == null)
                    continue;
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyValue,
                              nestingLevel + 1));
            }
            return sb.ToString();
        }

        private object ApplyConfigs(object value, List<Func<object, string>> configs)
        {
            var result = value;
            foreach (var config in configs)
                result = config(result);
            return result;
        }

        public ImmutableDictionary<object, List<Func<object, string>>> AddConfig(object key, Func<object, string> value)
        {
            var newConfigs = configs;
            if (!configs.ContainsKey(key))
                newConfigs = configs.Add(key, new List<Func<object, string>>());
            newConfigs[key].Add(value);
            return newConfigs;
        }

        public PrintingConfig<TOwner> Exclude<T>()
        {
            var type = typeof(T);
            var newConfigs = AddConfig(type, p => null);
            return new PrintingConfig<TOwner>(newConfigs);
        }

        public PrintingConfig<TOwner> Exclude(string propertyName)
        {
            var newConfigs = AddConfig(propertyName, p => null);
            return new PrintingConfig<TOwner>(newConfigs);
        }

        public SerializePrintingConfig<TOwner, string> Serialize(string propertyName)
        {
            return new SerializePrintingConfig<TOwner, string>(this, propertyName);
        }

        public SerializePrintingConfig<TOwner, T> Serialize<T>()
        {
            var type = typeof(T);
            return new SerializePrintingConfig<TOwner, T>(this, type);
        }

        public CulturePrintingConfig<TOwner> SetCulture(CultureInfo cultureInfo)
        {
            return new CulturePrintingConfig<TOwner>();
        }
    }
}