using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Solved;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly Config config;
        private PropertyInfo selectedProperty;

        public PrintingConfig()
        {
            config = new Config();
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public PrintingConfig<TOwner> TypeSerializer<TProperty>(Func<object, string> func)
        {
            config.typesSerializer.Add(typeof(TProperty), func);
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TProperty>()
        {
            config.excludedTypes.Add(typeof(TProperty));
            return this;
        }


        public SelectedProperty<TOwner, TProperty> Choose<TProperty>(Expression<Func<TOwner, TProperty>> selector)
        {
            selectedProperty = (PropertyInfo) ((MemberExpression) selector.Body).Member;
            return new SelectedProperty<TOwner, TProperty>(selectedProperty, this, config);
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

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (config.excludedTypes.Contains(propertyInfo.PropertyType))
                    continue;
                if (config.exludedFields.Contains(propertyInfo))
                    continue;

                var current = propertyInfo.GetValue(obj).ToString();
                if (config.numbersCulture.ContainsKey(propertyInfo))
                    current = string.Format(config.numbersCulture[propertyInfo], current);

                if (config.typesSerializer.ContainsKey(propertyInfo.PropertyType))
                {
                    var valueToString = config.typesSerializer[propertyInfo.PropertyType]
                        .DynamicInvoke(current)
                        ?.ToString();
                    sb.Append(indentation + propertyInfo.Name + " = " + valueToString);
                }

                if (config.fieldSerializers.ContainsKey(propertyInfo))
                {
                    var textToAdd = config.fieldSerializers[propertyInfo]
                        .DynamicInvoke(current)
                        ?.ToString();
                    sb.Append(indentation + propertyInfo.Name + " = " +
                              textToAdd);
                }
                else
                {
                    sb.Append(indentation + propertyInfo.Name + " = " +
                              PrintToString(propertyInfo.GetValue(obj),
                                  nestingLevel + 1));
                }
            }

            return sb.ToString();
        }
    }
}