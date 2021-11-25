using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectPrinting.Contexts
{
    public class ConfigPrintingContext<TOwner>
    {
        private readonly PrintingConfig config;

        public ConfigPrintingContext(PrintingConfig config)
        {
            this.config = config;
        }

        public ConfigPrintingContext<TOwner> Excluding<TPropType>() =>
            new(config with
            {
                ExcludingTypes = config.ExcludingTypes.Add(typeof(TPropType))
            });

        public ConfigPrintingContext<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> propertySelector)
        {
            var propertyInfo = (PropertyInfo)((MemberExpression)propertySelector.Body).Member;
            return new ConfigPrintingContext<TOwner>(config with
            {
                ExcludingProperties = config.ExcludingProperties.Add(propertyInfo)
            });
        }

        public TypePrintingContext<TOwner, TPropType> Printing<TPropType>() => new(config);

        public PropertyPrintingContext<TOwner> Printing<TPropType>(Expression<Func<TOwner, TPropType>> propertySelector)
        {
            var propertyInfo = (PropertyInfo)((MemberExpression)propertySelector.Body).Member;
            var propertyConfig = new PropertyPrintingContext<TOwner>(config, propertyInfo);
            return propertyConfig;
        }

        public ConfigPrintingContext<TOwner> FormatFor<TPropType>(IFormatProvider format)
            where TPropType : IFormattable =>
            new(config with
            {
                TypePrinting =
                config.TypePrinting.SetItem(typeof(TPropType), obj => ((TPropType)obj).ToString("", format))
            });

        public ConfigPrintingContext<TOwner> MaxStringLength(int maxLength)
        {
            var ellipsis = '\u2026'.ToString();
            Func<object, string> trim = obj =>
            {
                var line = (string)obj;
                return line.Length <= maxLength
                    ? line
                    : line[..maxLength] + ellipsis;
            };

            return new ConfigPrintingContext<TOwner>(config with
            {
                TypePrinting = config.TypePrinting.SetItem(typeof(string), trim)
            });
        }

        public string PrintToString(TOwner obj) => new ObjectPrinter<TOwner>(config).PrintToString(obj);
    }
}