using NUnit.Framework.Constraints;
using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Common
{
    public interface IPrintingConfig<TOwner> : IHaveRoot
    {
        private static readonly Type[] finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        private static readonly Type[] numericTypes = new[]
        {
            typeof(double), typeof(float)
        };

        private static readonly string propertyFormat = "{0}{1} = {2}";

        public string PrintToString<T>(T obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            var currentType = obj.GetType();
            if (Root.TypeSerializers.ContainsKey(currentType))
                return Root.TypeSerializers[currentType](obj);

            if (numericTypes.Contains(currentType) && Root.NumericTypeCulture.ContainsKey(currentType))
            {
                var currentCulture = CultureInfo.CurrentCulture;

                CultureInfo.CurrentCulture = Root.NumericTypeCulture[currentType];
                var str = obj.ToString();

                CultureInfo.CurrentCulture = currentCulture;
                return str;
            }

            if (finalTypes.Contains(currentType))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            sb.AppendLine(currentType.Name);
            var properties = currentType.GetProperties();
            if (properties.Length < 1)
                return sb.Append(obj.ToString()).ToString();

            foreach (var propertyInfo in currentType.GetProperties())
            {
                if (Root.ExcludedTypes.Contains(propertyInfo.PropertyType))
                    continue;

                if (Root.ExcludedProperties.Contains(propertyInfo))
                    continue;

                if (Root.PropertySerializers.ContainsKey(propertyInfo))
                {
                    var serializer = Root.PropertySerializers[propertyInfo];
                    sb.AppendFormat(propertyFormat, identation,
                                                    propertyInfo.Name,
                                                    serializer(propertyInfo.GetValue(obj))).AppendLine();
                    continue;
                }

                sb.AppendFormat(propertyFormat, identation,
                                                propertyInfo.Name,
                                                PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1));
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
}