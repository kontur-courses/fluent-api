using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Extensions;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private readonly PrintingConfigSettings settings;

        PrintingConfigSettings IPrintingConfig.Settings => settings;

        public PrintingConfig(PrintingConfigSettings settings)
        {
            this.settings = settings;
        }

        public TypePrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public ConcretePropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyInfo = ((MemberExpression) memberSelector.Body).Member as PropertyInfo;
            return new ConcretePropertyPrintingConfig<TOwner, TPropType>(this, propertyInfo);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyInfo = ((MemberExpression)memberSelector.Body).Member as PropertyInfo;
            return new PrintingConfig<TOwner>(settings.AddPropertyToIgnore(propertyInfo));
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            return new PrintingConfig<TOwner>(settings.AddTypeToIgnore(typeof(TPropType)));
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (obj is ICollection collection)
                return PrintCollection(collection, nestingLevel);

            var type = obj.GetType();
            if (settings.WaysToSerializeTypes.ContainsKey(type))
                return settings.WaysToSerializeTypes[type](obj) + Environment.NewLine;

            if (IsFinalType(type))
            {
                return (settings.TypesCultures.ContainsKey(type)
                           ? string.Format(settings.TypesCultures[type], "{0}", obj)
                           : obj) +
                       Environment.NewLine;
            }

            return PrintProperties(obj, nestingLevel);
        }

        private bool IsFinalType(Type type)
        {
            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            return finalTypes.Contains(type);
        }

        private string PrintProperties(object obj, int nestingLevel)
        {
            var type = obj.GetType();
            var indentation = new string('\t', nestingLevel + 1);
            var builder = new StringBuilder();
            builder.AppendLine(type.Name);
            foreach (var propertyInfo in GetAllowedPropertiesForType(type))
            {
                var propertyValue = PrintProperty(obj, nestingLevel, propertyInfo);
                builder.Append($"{indentation}{propertyInfo.Name} = {propertyValue}");
            }
            return builder.ToString();
        }

        private IEnumerable<PropertyInfo> GetAllowedPropertiesForType(Type type)
        {
            return type.GetProperties()
                .Where(p => !settings.TypesToIgnore.Contains(p.PropertyType) &&
                            !settings.PropertiesToIgnore.Contains(p));
        }

        private string PrintProperty(object obj, int nestingLevel, PropertyInfo propertyInfo)
        {
            var propertyValue = settings.WaysToSerializeProperties.ContainsKey(propertyInfo)
                ? settings.WaysToSerializeProperties[propertyInfo](propertyInfo.GetValue(obj))
                : PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);

            if (settings.MaxLengthsOfProperties.ContainsKey(propertyInfo))
                propertyValue = propertyValue.Truncate(settings.MaxLengthsOfProperties[propertyInfo]);

            return propertyValue;
        }

        private string PrintCollection(ICollection collection, int nestingLevel)
        {
            var builder = new StringBuilder();
            builder.Append("[");
            foreach (var element in collection)
            {
                builder.Append(PrintToString(element, nestingLevel));
            }

            builder.Append("]");
            return builder.ToString();
        }
    }
}