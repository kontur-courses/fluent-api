using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : Config
    {   
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
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
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            
            foreach (var propertyInfo in type.GetProperties())
            {
                var propertyType = propertyInfo.PropertyType;
                
                if (excludingTypes.Contains(propertyType) ||
                    excludingProperties.Contains(propertyInfo))
                    continue;

                string represent;

                if (typePrinters.ContainsKey(propertyType))
                    represent = typePrinters[propertyType](obj);
                else
                {
                    var propertyValue = propertyInfo.GetValue(obj);

                    if (trimmedProperties.ContainsKey(propertyInfo))
                    {
                        var length = trimmedProperties[propertyInfo];
                        
                        if (length < propertyValue.ToString().Length)
                            propertyValue = propertyValue.ToString()
                                .Substring(0, length);
                    }

                    represent = indentation + propertyInfo.Name + " = " +
                              PrintToString(propertyValue,
                                  nestingLevel + 1);
                }

                sb.Append(represent);
            }
            
            return sb.ToString();
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

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> printer)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, printer);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(
            Expression<Func<TOwner, TPropType>> propertySelector)
        {
            var propertyInfo = (PropertyInfo)((MemberExpression)propertySelector.Body).Member;
            excludingProperties.Add(propertyInfo);
            return this;
        }
    }
}