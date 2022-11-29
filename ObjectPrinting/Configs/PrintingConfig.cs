using System;
using System.Linq;
using System.Text;
using ObjectPrinting.BasicConfigurator;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly IBasicConfigurator<TOwner> configurator;
        private readonly Type[] finalTypes = {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };
        
        public PrintingConfig(IBasicConfigurator<TOwner> configurator)
        {
            this.configurator = configurator;
        }
        
        public string PrintToString(TOwner obj) =>
            PrintToString(obj, 0);

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj is null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            var result = obj + Environment.NewLine;
            if (configurator.TypeConfigs.ContainsKey(type))
                result = ConfigureItem(result, configurator.TypeConfigs[type]);

            if (finalTypes.Contains(type))
                return result;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                var value = PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
                if (configurator.MemberInfoConfigs.ContainsKey(propertyInfo))
                    value = ConfigureItem(value, configurator.MemberInfoConfigs[propertyInfo]);
                    
                if (configurator.ExcludedTypes.Contains(propertyInfo.PropertyType) || configurator.ExcludedMembers.Contains(propertyInfo))
                    continue;
                sb.Append(identation + propertyInfo.Name + " = " + value);
            }

            return sb.ToString();
        }

        private static string ConfigureItem(string result, UniversalConfig config)
        {
            result = result.ToString(config.CultureInfo);
            if (config.TrimLength > 0)
                result = result[..config.TrimLength] + Environment.NewLine;

            return result;
        }
    }
}