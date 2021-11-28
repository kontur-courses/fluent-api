using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludingTypes;
        private readonly Dictionary<Type, Delegate> typePrints;
            
        public PrintingConfig()
        {
            excludingTypes = new HashSet<Type>();
            typePrints = new Dictionary<Type, Delegate>();
        }
        
        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludingTypes.Add(typeof(TPropType));
            return this;
        }
        
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public void UpdatePrintType(Type type, Delegate print)
        {
            typePrints[type] = print;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            
            var type = obj.GetType();
            if (finalTypes.Contains(type))
            {
                if (typePrints.ContainsKey(type))
                    return typePrints[type].DynamicInvoke(obj) + Environment.NewLine;
                return obj + Environment.NewLine;
            }

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludingTypes.Contains(propertyInfo.PropertyType))
                    continue;
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(
                              propertyInfo.GetValue(obj),
                              nestingLevel + 1
                          )
                );
            }

            return sb.ToString();
        }
    }
}