using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ObjectPrinting.Contexts;

namespace ObjectPrinting
{
    public static class ObjectPrinter
    {
        public static ConfigPrintingContext<T> For<T>() => new(new PrintingConfig
        {
            FinalTypes = new List<Type>
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            }
        });
    }

    public class ObjectPrinter<TOwner>
    {
        private readonly PrintingConfig config;

        public ObjectPrinter(PrintingConfig config)
        {
            this.config = config;
        }

        public string PrintToString(TOwner obj) => PrintToString(obj, 0);

        private string PrintToString(object obj, int nestingLevel) =>
            TryGetTypeString(obj, out var typeString)
                ? typeString
                : GetObjectString(obj, nestingLevel);

        private string GetObjectString(object obj, int nestingLevel)
        {
            var builder = new StringBuilder();
            var type = obj.GetType();
            builder.AppendLine(type.Name);

            AppendProperties(builder, obj, nestingLevel);

            return builder.ToString();
        }

        private void AppendProperties(StringBuilder builder, object obj, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            foreach (var propertyInfo in ExcludeProperties(obj.GetType().GetProperties()))
            {
                builder.Append(indentation);
                if (TryGetPropertyString(obj, propertyInfo, out var propString))
                {
                    builder.Append(propString);
                    continue;
                }

                builder.Append(propertyInfo.Name + " = ");
                if (TryGetTypeString(obj, out var typeString))
                {
                    builder.Append(typeString);
                    continue;
                }

                builder.Append(PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1));
            }
        }

        private IEnumerable<PropertyInfo> ExcludeProperties(IEnumerable<PropertyInfo> properties) =>
            properties
                .Where(propInfo => !config.ExcludingTypes.Contains(propInfo.PropertyType))
                .Where(propInfo => !config.ExcludingProperties.Contains(propInfo));

        private bool TryGetPropertyString(object obj, PropertyInfo propertyInfo, out string propString)
        {
            if (config.PropertyPrinting.TryGetValue(propertyInfo, out var printProperty))
            {
                propString = printProperty(propertyInfo) + Environment.NewLine;
                return true;
            }

            propString = null;
            return false;
        }

        private bool TryGetTypeString(object obj, out string typeString)
        {
            if (obj == null)
            {
                typeString = "null" + Environment.NewLine;
                return true;
            }

            var type = obj.GetType();
            if (config.TypePrinting.TryGetValue(type, out var printType))
            {
                typeString = printType(obj) + Environment.NewLine;
                return true;
            }

            if (config.FinalTypes.Contains(type))
            {
                typeString = obj + Environment.NewLine;
                return true;
            }

            typeString = null;
            return false;
        }
    }
}