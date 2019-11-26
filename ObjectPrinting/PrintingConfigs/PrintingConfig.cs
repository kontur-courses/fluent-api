using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using ObjectPrinting.Formatting;
using ObjectPrinting.Serializer;

namespace ObjectPrinting.PrintingConfigs
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private readonly List<PropertySerializationRule> serializationRules;
        private FormatConfiguration installedFormatting = null;

        public PrintingConfig()
        {
            serializationRules = new List<PropertySerializationRule>();
        }

        public string PrintToString(object obj)
        {
            return ObjectPrinter.PrintToString(obj, this);
        }

        public PrintingConfig<TOwner> SetFormatting(FormatConfiguration configuration)
        {
            installedFormatting = configuration;
            return this;
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            serializationRules.Add(
                new PropertySerializationRule((obj, propertyInfo) => propertyInfo.PropertyType == typeof(T),
                null));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> func)
        {
            var propInfo = ((MemberExpression) func.Body).Member as PropertyInfo;

            serializationRules.Add(
                new PropertySerializationRule((obj, propertyInfo) => propertyInfo.Name == propInfo?.Name,
                null));
            return this;
        }

        public PropertyPrintingConfig<TOwner, T> Printing<T>()
        {
            return new PropertyPrintingConfig<TOwner, T>(this);
        }

        public PropertyPrintingConfig<TOwner, T> Printing<T>(Expression<Func<TOwner, T>> func)
        {
            return new PropertyPrintingConfig<TOwner, T>(this, func);
        }

        IReadOnlyList<PropertySerializationRule> IPrintingConfig.SerializationRules => serializationRules;
        FormatConfiguration IPrintingConfig.InstalledFormatting => installedFormatting;
        void IPrintingConfig.ApplyNewSerializationRule(PropertySerializationRule rule) => serializationRules.Add(rule);
    }
}