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
                var hasTypeConfig = Configs.TryGetValue(propertyInfo.PropertyType, out var configsByType);
                if (hasTypeConfig)
                    propertyValue = ApplyConfigs(propertyValue, configsByType);
                var hasNameConfig = Configs.TryGetValue(propertyInfo, out var configsByName);
                if (hasNameConfig)
                    propertyValue = ApplyConfigs(propertyValue, configsByName);
                if (propertyValue == null)
                    continue;
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyValue,
                              nestingLevel + 1));
            }
            return sb.ToString();
        }

        private object ApplyConfigs(object value, IEnumerable<Delegate> configs)
        {
            var result = value;
            foreach (var @delegate in configs)
            {
                result = @delegate.DynamicInvoke(result);
            }

            return result;
            //return configs
            //    .Aggregate(value, (current, config) => config.DynamicInvoke(current));
        }

        public ImmutableDictionary<object, List<Delegate>> AddConfig(object key, Func<object, string> value)
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
            var newConfigs = AddConfig(type, p => null);
            return new PrintingConfig<TOwner>(newConfigs);
        }

        public PrintingConfig<TOwner> Exclude<TProp>(Expression<Func<TOwner, TProp>> property)
        {
            var propertyInfo = ((MemberExpression)property.Body).Member as PropertyInfo;
            var newConfigs = AddConfig(propertyInfo, p => null);
            return new PrintingConfig<TOwner>(newConfigs);
        }

        public SerializePrintingConfig<TOwner, string> Serialize(Expression<Func<TOwner, string>> property)
        {
            var propertyInfo = ((MemberExpression) property.Body).Member as PropertyInfo;
            return new SerializePrintingConfig<TOwner, string>(this, propertyInfo);
        }

        public SerializePrintingConfig<TOwner, T> Serialize<T>()
        {
            var type = typeof(T);
            return new SerializePrintingConfig<TOwner, T>(this, type);
        }

        public PrintingConfig<TOwner> SetCulture(CultureInfo cultureInfo)
        {
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            return new PrintingConfig<TOwner>(Configs);
        }
    }
}