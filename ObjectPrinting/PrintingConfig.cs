using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    internal class PrintingConfigRoot
    {
        internal HashSet<Type> ExcludedTypes { get; set; } = new HashSet<Type>();
        internal Dictionary<Type, Func<object, string>> TypeSerializers { get; set; } = new Dictionary<Type, Func<object, string>>();
        internal Dictionary<Type, CultureInfo> NumericTypeCulture { get; set; } = new Dictionary<Type, CultureInfo>();
        internal Dictionary<PropertyInfo, Func<object, string>> PropertySerializers { get; } = new Dictionary<PropertyInfo, Func<object, string>>();
        internal HashSet<PropertyInfo> ExcludedProperties { get; } = new HashSet<PropertyInfo>();
        internal Dictionary<PropertyInfo, int> MaxStringPropertyLengths { get; } = new Dictionary<PropertyInfo, int>();
    }

    public interface IHaveRoot
    {
        internal PrintingConfigRoot Root { get; }
    }

    public interface IPrintingConfig<TOwner> : IHaveRoot
    {
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
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }

        public IPrintingConfig<TOwner> Exclude<TExcluded>()
        {
            Root.ExcludedTypes.Add(typeof(TExcluded));
            return this;
        }

        public IPrintingConfig<TOwner> SerializeTypeAs<T>(Func<T, string> serializationFunction)
        {
            var type = typeof(T);
            if (!Root.TypeSerializers.ContainsKey(type))
                Root.TypeSerializers.Add(type, obj => serializationFunction((T)obj));
            return this;
        }

        public IPrintingConfig<TOwner> SetNumericTypeCulture<T>(CultureInfo culture)
        {
            var type = typeof(T);
            if (!Root.NumericTypeCulture.ContainsKey(type))
                Root.NumericTypeCulture.Add(type, culture);

            return this;
        }

        public IPrintingPropertyBaseConfig<TOwner, T> ConfigurePropertySerialization<T>(Expression<Func<TOwner, T>> property)
        {
            var propertyInfo = GetPropertyInfoFromExpression(property.Body);
            return new PrintingPropertyBaseConfig<TOwner, T>(propertyInfo, Root);
        }

        public IPrintingConfig<TOwner> ExcludeProperty<T>(Expression<Func<TOwner, T>> property)
        {
            var propertyInfo = GetPropertyInfoFromExpression(property.Body);
            Root.ExcludedProperties.Add(propertyInfo);
            return this;
        }

        private static PropertyInfo GetPropertyInfoFromExpression(Expression expression)
        {
            if (!(expression is MemberExpression))
                throw new ArgumentException($"Can't find property {expression}");

            return (expression as MemberExpression).Member as PropertyInfo;
        }
    }

    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private PrintingConfigRoot root = new PrintingConfigRoot();
        PrintingConfigRoot IHaveRoot.Root { get => root; }
    }

    public interface IPrintingPropertyBaseConfig<TOwner, T> : IHaveRoot
    {
        internal PropertyInfo CurrentProperty { get; }

        public IPrintingPropertyConfig<TOwner, T> SetSerializer(Func<T, string> serializer)
        {
            var objSerializer = new Func<object, string>(obj => serializer((T)obj));

            if (!Root.PropertySerializers.ContainsKey(CurrentProperty))
                Root.PropertySerializers.Add(CurrentProperty, objSerializer);
            else Root.PropertySerializers[CurrentProperty] = objSerializer;

            return new PrintingPropertyConfig<TOwner, T>(CurrentProperty, Root);
        }
    }

    public class PrintingPropertyBaseConfig<TOwner, T> : IPrintingPropertyConfig<TOwner, T>
    {
        private PropertyInfo currentProperty;
        private PrintingConfigRoot root;

        PrintingConfigRoot IHaveRoot.Root => root;
        PropertyInfo IPrintingPropertyBaseConfig<TOwner, T>.CurrentProperty => currentProperty;

        internal PrintingPropertyBaseConfig(PropertyInfo property, PrintingConfigRoot root)
        {
            currentProperty = property;
            this.root = root;
        }
    }

    public interface IPrintingPropertyConfig<TOwner, T> : IPrintingConfig<TOwner>, IPrintingPropertyBaseConfig<TOwner, T>
    { }

    public class PrintingPropertyConfig<TOwner, T> : IPrintingPropertyConfig<TOwner, T>
    {
        private readonly PrintingConfigRoot root;
        private readonly PropertyInfo currentProperty;

        PrintingConfigRoot IHaveRoot.Root => root;
        PropertyInfo IPrintingPropertyBaseConfig<TOwner, T>.CurrentProperty => currentProperty;

        internal PrintingPropertyConfig(PropertyInfo property, PrintingConfigRoot root)
        {
            this.root = root;
            currentProperty = property;
        }
    }
}