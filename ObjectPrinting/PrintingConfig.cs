using System;
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
        public ConfigurationInfo ConfigurationInfo { get; }
        private readonly HashSet<object> serializedObjects;

        public PrintingConfig()
        {
            ConfigurationInfo = new ConfigurationInfo();
            serializedObjects = new HashSet<object>();
        }

        public PrintingConfig(ConfigurationInfo configurationInfo)
        {
            ConfigurationInfo = configurationInfo;
            serializedObjects = new HashSet<object>();
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>() =>
            new PropertyPrintingConfig<TOwner, TPropType>(this);

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector) =>
            new PropertyPrintingConfig<TOwner, TPropType>(this, memberSelector);

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector) =>
            new PrintingConfig<TOwner>(ConfigurationInfo.AddPropertyToExclude(memberSelector.GetPropertyInfo().Name));

        public PrintingConfig<TOwner> Excluding<TPropType>() =>
            new PrintingConfig<TOwner>(ConfigurationInfo.AddTypeToExclude(typeof(TPropType)));

        public string PrintToString(TOwner obj) => PrintToString(obj, 0);

        private string PrintToString(object obj, int nestingLevel)
        {
            if (serializedObjects.Contains(obj))
                return obj.GetType().Name + Environment.NewLine;;

            if (obj == null)
                return "null" + Environment.NewLine;

            if (FinalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            serializedObjects.Add(obj);
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (ConfigurationInfo.ShouldExclude(propertyInfo))
                    continue;
                sb.Append(identation + propertyInfo.Name + " = ");
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

            return sb.ToString();
        }
    }
}