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
        private static readonly PropertyInfo[] Properties = typeof(TOwner).GetProperties();
        private static readonly List<PropertyInfo> PropertiesToExclude = [];
        public PrintingConfig<TOwner> Excluding<TPropertyType>()
        {
            PropertiesToExclude
                .AddRange(Properties
                    .Where(propertyInfo => propertyInfo.PropertyType == typeof(TPropertyType)));

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
            
            PropertiesToExclude.Add(propertyInfo);
            
            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropertyType> Printing<TPropertyType>()
        {
            throw  new NotImplementedException();
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
            foreach (var propertyInfo in type.GetProperties().Where(prop => !PropertiesToExclude.Contains(prop)))
            {
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }
    }
}