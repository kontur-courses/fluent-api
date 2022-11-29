using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly IBasicConfigurator<TOwner> configurator;
        private readonly Type[] finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };
        
        public PrintingConfig(IBasicConfigurator<TOwner> configurator)
        {
            this.configurator = configurator;
        }
        
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj is null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            var result = obj + Environment.NewLine;
            if (configurator.TypeConfigs.ContainsKey(type))
            {
                result = result.ToString(configurator.TypeConfigs[type].CultureInfo);
                if (configurator.TypeConfigs[type].TrimLength > 0)
                    result = result![..configurator.TypeConfigs[type].TrimLength] + Environment.NewLine;
            }

            if (finalTypes.Contains(type))
                return result;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                var value = PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
                if (configurator.MemberInfoConfigs.ContainsKey(propertyInfo))
                {
                    if (value != null)
                    {
                        value = value.ToString(configurator.MemberInfoConfigs[propertyInfo].CultureInfo);
                        if (configurator.MemberInfoConfigs[propertyInfo].TrimLength > 0)
                            value = value[..configurator.MemberInfoConfigs[propertyInfo].TrimLength] + Environment.NewLine;
                    }
                }
                if (configurator.ExcludedTypes.Contains(propertyInfo.PropertyType) || configurator.ExcludedMembers.Contains(propertyInfo))
                    continue;
                // sb.Append(identation + propertyInfo.Name + " = " +
                //           PrintToString(value,
                //               nestingLevel + 1));
                sb.Append(identation + propertyInfo.Name + " = " + value);
            }

            return sb.ToString();
        }
    }
}