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
    public class PrintingConfig<TOwner>
    {
        internal HashSet<Type> ExcludedTypes { get; set; } = new HashSet<Type>();
        internal Dictionary<Type, Func<object, string>> TypeSerializers { get; set; } = new Dictionary<Type, Func<object, string>>();
        internal Dictionary<Type, CultureInfo> NumericTypeCulture { get; set; } = new Dictionary<Type, CultureInfo>();
        internal Dictionary<PropertyInfo, Func<object, string>> PropertySerializers { get; set; } = new Dictionary<PropertyInfo, Func<object, string>>();
        internal HashSet<PropertyInfo> ExcludedProperties { get; set; } = new HashSet<PropertyInfo>();

        internal PrintingConfig<TOwner> Parent { get; set; } = new PrintingConfig<TOwner>();

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

        public PrintingConfig<TOwner> Exclude<TExcluded>()
        {
            Parent.ExcludedTypes.Add(typeof(TExcluded));
            return this;
        }

        public PrintingConfig<TOwner> SerializeTypeAs<T>(Func<T, string> serializationFunction)
        {
            var type = typeof(T);
            if (!Parent.TypeSerializers.ContainsKey(type))
                Parent.TypeSerializers.Add(type, obj => serializationFunction((T)obj));

            return this;
        }

        public PrintingConfig<TOwner> SetNumericTypeCulture<T>(CultureInfo culture)
        {
            var type = typeof(T);
            if (!Parent.NumericTypeCulture.ContainsKey(type))
                Parent.NumericTypeCulture.Add(type, culture);

            return this;
        }

        public PrintingStringPropertySerializer<TOwner> ConfigurePropertySerialization(Expression<Func<TOwner, string>> property)
        {
            var propertyInfo = GetPropertyInfoFromExpression(property);
            return new PrintingStringPropertySerializer<TOwner>(propertyInfo, Parent);
        }

        public PrintingPropertySerializer<TOwner, T> ConfigurePropertySerialization<T>(Expression<Func<TOwner, T>> property)
        {
            var propertyInfo = GetPropertyInfoFromExpression(property);
            return new PrintingPropertySerializer<TOwner, T>(propertyInfo, Parent);
        }

        public PrintingConfig<TOwner> ExcludeProperty<T>(Expression<Func<TOwner, T>> property)
        {
            var propertyInfo = GetPropertyInfoFromExpression(property);
            Parent.ExcludedProperties.Add(propertyInfo);
            return this;
        }

        private static PropertyInfo GetPropertyInfoFromExpression(Expression expression)
        {
            if (!(expression is MemberExpression))
                throw new ArgumentException("Argument expression doesn't contain property");

            return (expression as MemberExpression).Member as PropertyInfo;
        }
    }

    public class PrintingPropertySerializer<TOwner, T> : PrintingConfig<TOwner>
    {
        protected readonly PropertyInfo currentProperty;

        public PrintingPropertySerializer(PropertyInfo property, PrintingConfig<TOwner> parent)
        {
            currentProperty = property;
            Parent = parent;
        }

        public PrintingPropertySerializer<TOwner, T> SetSerializer(Func<T, string> serializer)
        {
            if (!Parent.PropertySerializers.ContainsKey(currentProperty))
                Parent.PropertySerializers.Add(currentProperty, obj => serializer((T)obj));

            return this;
        }
    }

    public class PrintingStringPropertySerializer<TOwner> : PrintingConfig<TOwner>
    {
        protected readonly PropertyInfo currentProperty;

        public PrintingStringPropertySerializer(PropertyInfo property, PrintingConfig<TOwner> parent)
        {
            currentProperty = property;
            Parent = parent;
        }

        public PrintingStringPropertySerializer<TOwner> SetSerializer(Func<string, string> serializer)
        {
            if (!Parent.PropertySerializers.ContainsKey(currentProperty))
                Parent.PropertySerializers.Add(currentProperty, obj => serializer((string)obj));

            return this;
        }

        public PrintingStringPropertySerializer<TOwner> SetMaxLength(int length)
        {
            if(Parent.PropertySerializers.ContainsKey(currentProperty))
            {
                Parent.PropertySerializers[currentProperty] = 
                    obj => Parent.PropertySerializers[currentProperty](obj).Substring(length);
                return this;
            }

            Parent.PropertySerializers.Add(currentProperty, obj => ((string)obj).Substring(length));
            return this;
        }
    }
}