using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public readonly ImmutableDictionary<object, List<Delegate>> Configs;

        public PrintingConfig()
        {
            Configs = ImmutableDictionary<object, List<Delegate>>.Empty;
        }

        public PrintingConfig(ImmutableDictionary<object, List<Delegate>> configs)
        {
            Configs = configs;
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
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                var propertyValue = propertyInfo.GetValue(obj);
                if (Configure(propertyInfo.PropertyType, propertyValue, out var result))
                    propertyValue = result;
                if (Configure(propertyInfo, propertyValue, out result))
                    propertyValue = result;
                if (propertyValue is Excluded)
                    continue;
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyValue,
                              nestingLevel + 1));
            }
            return sb.ToString();
        }

        private bool Configure(object key, object propertyValue, out object result)
        {
            var hasConfig = Configs.TryGetValue(key, out var configsByName);
            result = hasConfig ? ApplyConfigs(propertyValue, configsByName) : null;
            return hasConfig;
        }

        private object ApplyConfigs(object value, IEnumerable<Delegate> configs)
        {
            return configs
                .Aggregate(value, (current, config) => config.DynamicInvoke(current));
        }

        public ImmutableDictionary<object, List<Delegate>> AddConfig(object key, Func<object, object> value)
        {
            var newConfigs = Configs;
            if (!Configs.ContainsKey(key))
                newConfigs = Configs.Add(key, new List<Delegate>());
            newConfigs[key].Add(value);
            return newConfigs;
        }

        public PrintingConfig<TOwner> Exclude<T>()
        {
            var type = typeof(T);
            var newConfigs = AddConfig(type, p => new Excluded());
            return new PrintingConfig<TOwner>(newConfigs);
        }

        public PrintingConfig<TOwner> Exclude<TProp>(Expression<Func<TOwner, TProp>> property)
        {
            var propertyInfo = ((MemberExpression)property.Body).Member as PropertyInfo;
            var newConfigs = AddConfig(propertyInfo, p => new Excluded());
            return new PrintingConfig<TOwner>(newConfigs);
        }

        public SerializePrintingConfig<TOwner, TProp> Serialize<TProp>(Expression<Func<TOwner, TProp>> property)
        {
            var propertyInfo = ((MemberExpression) property.Body).Member as PropertyInfo;
            return new SerializePrintingConfig<TOwner, TProp>(this, propertyInfo);
        }

        public SerializePrintingConfig<TOwner, T> Serialize<T>()
        {
            var type = typeof(T);
            return new SerializePrintingConfig<TOwner, T>(this, type);
        }

        public PrintingConfig<TOwner> SetCultureFor<T>(CultureInfo cultureInfo)
        {
            var newConfig = AddConfig(typeof(T), o => string.Format(cultureInfo, "{0}", o));
            return new PrintingConfig<TOwner>(newConfig);
        }
    }
}