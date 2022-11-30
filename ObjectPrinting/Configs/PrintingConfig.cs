using System;
using System.Linq;
using System.Text;
using ObjectPrinting.ObjectConfiguration.Implementation;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly ObjectConfiguration<TOwner> configuration;
        private readonly Type[] finalTypes = {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };
        
        public PrintingConfig(ObjectConfiguration<TOwner> configuration)
        {
            this.configuration = configuration;
        }
        
        public string PrintToString(TOwner obj) =>
            PrintToString(obj, 0);

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj is null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            var result = obj + Environment.NewLine;
            if (configuration.TypeConfigs.ContainsKey(type))
                result = ConfigureItem(result, configuration.TypeConfigs[type]);

            if (finalTypes.Contains(type))
                return result;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                var value = PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
                if (configuration.MemberInfoConfigs.ContainsKey(propertyInfo))
                    value = ConfigureItem(value, configuration.MemberInfoConfigs[propertyInfo]);
                    
                if (configuration.ExcludedTypes.Contains(propertyInfo.PropertyType) || configuration.ExcludedMembers.Contains(propertyInfo))
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