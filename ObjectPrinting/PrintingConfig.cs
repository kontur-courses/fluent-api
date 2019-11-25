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
        public PrintingConfig(PrintingConfig<TOwner> parent)
        {
            parent.excludedTypes.ForEach(t => excludedTypes.Add(t));
            parent.excludedProperties.ForEach(t => excludedProperties.Add(t));
            parent.typeSerializators.ForEach(t => typeSerializators.Add(t.Key, t.Value));
        }

        public PrintingConfig()
        {
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        protected HashSet<Type> excludedTypes = new HashSet<Type>();
        protected HashSet<PropertyInfo> excludedProperties = new HashSet<PropertyInfo>();

        public Dictionary<Type, Func<Object, string>> typeSerializators =
            new Dictionary<Type, Func<Object, string>>();

        public Dictionary<PropertyInfo, Func<Object, string>> propertySerializators =
            new Dictionary<PropertyInfo, Func<Object, string>>();

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

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (!excludedTypes.Contains(propertyInfo.PropertyType))
                    if (!excludedProperties.Contains(propertyInfo))
                    {
                        string serializedProperty = null;
                        if (propertySerializators.ContainsKey(propertyInfo))
                        {
                            serializedProperty = propertySerializators[propertyInfo](propertyInfo.GetValue(obj));
                        }
                        else if (typeSerializators.ContainsKey(propertyInfo.PropertyType))
                        {
                            serializedProperty =
                                typeSerializators[propertyInfo.PropertyType](propertyInfo.GetValue(obj));
                        }
                        else
                        {
                            serializedProperty = PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
                        }

                        sb.Append(indentation + propertyInfo.Name + " = " + serializedProperty);
                    }
            }

            return sb.ToString();
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            var childConfig = new PrintingConfig<TOwner>(this);
            childConfig.excludedTypes.Add(typeof(T));
            return childConfig;
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> func)
        {
            var childConfig = new PrintingConfig<TOwner>(this);
            var propertyInfo = ((MemberExpression) func.Body)
                .Member as PropertyInfo;
            childConfig.excludedProperties.Add(propertyInfo);
            return childConfig;
        }

        public TypeSerializationConfig<TOwner, T> Serializing<T>()
        {
            return new TypeSerializationConfig<TOwner, T>(this);
        }

        public PropertySerializationConfig<TOwner, T> Serializing<T>(Expression<Func<TOwner, T>> propertyProvider)
        {
            return new PropertySerializationConfig<TOwner, T>(this, propertyProvider);
        }

        public PropertySerializationConfig<TOwner, string> Serializing(
            Expression<Func<TOwner, string>> propertyProvider)
        {
            return new PropertySerializationConfig<TOwner, string>(this, propertyProvider);
        }

        public PropertySerializationConfig<TOwner, int> Serializing(Expression<Func<TOwner, int>> propertyProvider)
        {
            return new PropertySerializationConfig<TOwner, int>(this, propertyProvider);
        }
    }

    public class PropertySerializationConfig<TOwner, TTarget> : IPropertyPrintingConfig<TOwner>
    {
        protected PrintingConfig<TOwner> config;
        protected PropertyInfo propertyInfo;
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.Config => config;

        public PropertySerializationConfig(PrintingConfig<TOwner> config,
            Expression<Func<TOwner, TTarget>> propertyProvider)
        {
            this.config = config;
            this.propertyInfo = ((MemberExpression) propertyProvider.Body).Member as PropertyInfo;
        }

        public PrintingConfig<TOwner> Using(Func<TTarget, string> serializer)
        {
            config.propertySerializators.Add(propertyInfo, t => serializer((TTarget) t));
            return config;
        }
    }

    public class TypeSerializationConfig<TOwner, TTarget> : IPropertyPrintingConfig<TOwner>
    {
        protected PrintingConfig<TOwner> config;
        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner>.Config => config;

        public TypeSerializationConfig(PrintingConfig<TOwner> config)
        {
            this.config = config;
        }

        public PrintingConfig<TOwner> Using(Func<TTarget, string> serializer)
        {
            config.typeSerializators.Add(typeof(TTarget), t => serializer((TTarget) t));
            return config;
        }
    }

    public interface IPropertyPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> Config { get; }
    }

    public static class PrintingExtensions
    {
        public static PrintingConfig<TOwner> Using<TOwner>(this TypeSerializationConfig<TOwner, int> config,
            CultureInfo info)
        {
            var cfg = (config as IPropertyPrintingConfig<TOwner>).Config;
            cfg.typeSerializators.Add(typeof(int), t => ((int) t).ToString(info));
            return cfg;
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            using (var enumerator = enumerable.GetEnumerator())
                do
                {
                    action.Invoke(enumerator.Current);
                } while (enumerator.MoveNext());
        }
    }
}