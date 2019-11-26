using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public PrintingConfig()
        {
            excludedProps = new HashSet<PropertyInfo>();
            excludedTypes = new HashSet<Type>();
            typeAltPrintings = new Dictionary<Type, Func<object, string>>();
            propAltPrintings = new Dictionary<PropertyInfo, Func<object, string>>();
            finalTypes = new[] {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        private readonly Dictionary<Type, Func<object, string>> typeAltPrintings;
        
        public void AddTypeAltPrinting(Type propType, Func<object, string> printingData)
        {
            typeAltPrintings[propType] = printingData;
        }
        
        private readonly Dictionary<PropertyInfo, Func<object, string>> propAltPrintings;
        public void AddPropAltPrinting(PropertyInfo propType, Func<object, string> printingData)
        {
            propAltPrintings[propType] = printingData;
        }
        
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propInfo = ((MemberExpression)memberSelector.Body).Member as PropertyInfo;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propInfo);
        }

        private readonly HashSet<PropertyInfo> excludedProps;
        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propInfo = ((MemberExpression)memberSelector.Body).Member as PropertyInfo;
            excludedProps.Add(propInfo);
            return this;
        }

        private readonly HashSet<Type> excludedTypes;
        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private readonly Type[] finalTypes;
        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            
            var type = obj.GetType();
            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;

            var identation = string.Concat(Enumerable.Repeat(new string(' ', 4),
                                                             nestingLevel + 1));
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            
            foreach (var propertyInfo in type.GetProperties())
            {
                if (IsExcluded(propertyInfo))
                    continue;
                
                var propertyType = propertyInfo.PropertyType;
                var propertyValue = propertyInfo.GetValue(obj);
                
                string objPrinted;
                if (propAltPrintings.ContainsKey(propertyInfo))
                {
                    var altPrint = propAltPrintings[propertyInfo];
                    objPrinted = altPrint(propertyValue) + Environment.NewLine;
                }
                else if (typeAltPrintings.ContainsKey(propertyType))
                {
                    var altPrint = typeAltPrintings[propertyType];
                    objPrinted = altPrint(propertyValue) + Environment.NewLine;
                }
                else
                {
                    objPrinted = PrintToString(propertyValue,nestingLevel + 1);
                }
                sb.Append(identation + propertyInfo.Name + " = " + objPrinted);
            }
            return sb.ToString();
        }

        private bool IsExcluded(PropertyInfo propertyInfo)
        {
            return excludedProps.Contains(propertyInfo) || excludedTypes.Contains(propertyInfo.PropertyType);
        }
    }
}