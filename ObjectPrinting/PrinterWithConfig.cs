using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrinterWithConfig<TOwner>
    {
        private const int MaxNestingLevel = 10;

        private readonly HashSet<object> printedObjects = new();
        private readonly PrintingRules rules;
        private readonly HashSet<Type> finalTypes = new()
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        internal PrinterWithConfig(PrintingRules rules)
        {
            this.rules = rules;
        }

        private bool ShouldIgnore(PropertyInfo propertyInfo, int nestingLevel)
        {
            return rules.ShouldIgnoreInAllNestingLevel(propertyInfo.PropertyType)
                   || nestingLevel == 0 && rules.ShouldIgnore(propertyInfo.PropertyType)
                   || nestingLevel == 0 && rules.ShouldIgnore(propertyInfo.Name);
        }

        private bool TryGetSerialisationRuleFor(PropertyInfo propertyInfo, [NotNullWhen(true)] out SerialisationRule? serialisationRule)
        {
            return rules.TryGetSerialisationMethodForProperty(propertyInfo.Name, out serialisationRule) 
                   || rules.TryGetSerialisationMethodForType(propertyInfo.PropertyType, out serialisationRule);
        }

        private string? TryPrintPropertyInfo(object obj, PropertyInfo propertyInfo, int nestingLevel)
        {
            if (MaxNestingLevel == nestingLevel) return null;

            var name = propertyInfo.Name + " = ";
            var indentation = new string('\t', nestingLevel + 1);
            var value = propertyInfo.GetValue(obj);

            if (ShouldIgnore(propertyInfo, nestingLevel)) return null;
            if (TryGetSerialisationRuleFor(propertyInfo, out var serialisationRule)) 
                return indentation + name + serialisationRule.ToString(value) + Environment.NewLine;
            if (value is null) return indentation + name + "null" + Environment.NewLine;

            
            if (finalTypes.Contains(propertyInfo.PropertyType))
            {
                return indentation + name + (propertyInfo.PropertyType.GetInterfaces().Contains(typeof(IConvertible))
                    ? ((IConvertible) value).ToString(rules.GetCultureForType(propertyInfo.PropertyType)) : value) + Environment.NewLine;
            }
            
            
            var sb = new StringBuilder();
            sb.AppendLine(indentation + propertyInfo.PropertyType.Name);
        
            printedObjects.Add(value);
            foreach (var nestedPropertyInfo in propertyInfo.PropertyType.GetProperties())
            {
                if (printedObjects.Any(x => ReferenceEquals(x, nestedPropertyInfo.GetValue(value)))) return null;

                var nested = TryPrintPropertyInfo(value, nestedPropertyInfo, nestingLevel + 1);
                sb.Append(nested);
            }
            return sb.ToString();
        }

        public string PrintToString(TOwner? obj)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
        
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;
            
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            
            foreach (var propertyInfo in type.GetProperties())
            {
                sb.Append(TryPrintPropertyInfo(obj, propertyInfo, 0));
            }
            return sb.ToString();
        }
    }
}