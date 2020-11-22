using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private static readonly Type[] FinalTypes =
            {typeof(int), typeof(double), typeof(float), typeof(string), typeof(DateTime), typeof(TimeSpan)};

        internal ConfigurationInfo ConfigurationInfo { get; }
        private readonly HashSet<object> serializedObjects;

        public PrintingConfig()
        {
            ConfigurationInfo = new ConfigurationInfo();
            serializedObjects = new HashSet<object>();
        }

        internal PrintingConfig(ConfigurationInfo configurationInfo)
        {
            ConfigurationInfo = configurationInfo;
            serializedObjects = new HashSet<object>();
        }

        internal PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>() =>
            new PropertyPrintingConfig<TOwner, TPropType>(this);

        internal PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector) =>
            new PropertyPrintingConfig<TOwner, TPropType>(this, memberSelector);

        internal PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector) =>
            new PrintingConfig<TOwner>(ConfigurationInfo.AddPropertyToExclude(memberSelector.GetPropertyInfo().Name));

        internal PrintingConfig<TOwner> Excluding<TPropType>() =>
            new PrintingConfig<TOwner>(ConfigurationInfo.AddTypeToExclude(typeof(TPropType)));

        internal string PrintToString(TOwner obj) => PrintToString(obj, 0);

        private string PrintToString(object obj, int nestingLevel)
        {
            if (serializedObjects.Contains(obj))
                return obj.GetType().Name + Environment.NewLine;
            ;

            if (obj == null)
                return "null" + Environment.NewLine;

            if (FinalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            serializedObjects.Add(obj);
            var sb = new StringBuilder();

            sb.Append(obj is IEnumerable
                ? CollectionToString((ICollection) obj, nestingLevel + 1)
                : PropertiesToString(obj, nestingLevel));

            return sb.ToString();
        }

        private StringBuilder CollectionToString(ICollection obj, int nestingLevel)
        {
            var sb = new StringBuilder();
            if (obj.Count != 0)
            {
                sb.Append(Environment.NewLine + new string('\t', nestingLevel) + '[' + Environment.NewLine);
                foreach (var elem in obj)
                {
                    sb.Append(new string('\t', nestingLevel));
                    sb.Append(PrintToString(elem, nestingLevel));
                }

                sb.Append(new string('\t', nestingLevel) + ']');
            }
            else
                sb.Append("Empty");

            sb.Append(Environment.NewLine);
            return sb;
        }

        private StringBuilder PropertiesToString(object obj, int nestingLevel)
        {
            var sb = new StringBuilder();
            sb.AppendLine(new string(obj.GetType().Name));
            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                if (ConfigurationInfo.ShouldExclude(propertyInfo))
                    continue;
                sb.Append(new string('\t', nestingLevel + 1) + propertyInfo.Name + " = ");
                var serialized = ConfigurationInfo.TryUseConfiguration(propertyInfo, obj);
                if (serialized is null)
                {
                    serialized = PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
                    sb.Append(serialized);
                }
                else
                {
                    sb.Append(serialized);
                    sb.Append(Environment.NewLine);
                }
            }

            return sb;
        }
    }
}