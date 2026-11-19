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
        private readonly PropertyInfo[] properties = typeof(TOwner).GetProperties();
        private readonly List<PropertyInfo> propertiesToExclude = [];
        public PrintingConfig<TOwner> Excluding<TPropertyType>()
        {
            var propertyType = typeof(TPropertyType);
            
            propertiesToExclude
                .AddRange(properties
                    .Where(propertyInfo => propertyInfo.PropertyType == propertyType));

            return this;
        }
        
        public PrintingConfig<TOwner> Excluding<TPropertyType>(Expression<Func<TOwner, TPropertyType>> memberSelector)
        {
            if (memberSelector.Body is not MemberExpression memberExpression)
            {
                throw new ArgumentException("Выражение должно указывать на свойство или поле.");
            }
            
            if (memberExpression.Member is not PropertyInfo propertyInfo)
            {
                throw new ArgumentException("Выражение указывает на поле, а не на свойство.");
            }
            
            propertiesToExclude.Add(propertyInfo);
            
            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropertyType> Printing<TPropertyType>()
        {



            return new PropertyPrintingConfig<TOwner, TPropertyType>();
        }
        
        public PropertyPrintingConfig<TOwner, TPropertyType> Printing<TPropertyType>(Expression<Func<TOwner, TPropertyType>> memberSelector)
        {
            throw  new NotImplementedException();
        }
        
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            
            foreach (var propertyInfo in properties.Where(prop => !propertiesToExclude.Contains(prop)))
            {
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            
            return sb.ToString();
        }
    }
}