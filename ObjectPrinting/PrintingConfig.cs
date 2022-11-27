using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<string> excludedMembers = new HashSet<string>();

        public PrintingConfig()
        {

        }
        private PrintingConfig(PrintingConfig<TOwner> parent)
        {
            excludedTypes.AddRange(parent.excludedTypes);
            excludedMembers.AddRange(parent.excludedMembers);
        }
            
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var config = new PrintingConfig<TOwner>(this);
            var memberExpression = (memberSelector.Body as MemberExpression); 
            config.excludedMembers.Add(memberExpression?.Member.Name);
            return config;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            var config = new PrintingConfig<TOwner>(this);
            config.excludedTypes.Add(typeof(TPropType));
            return config;
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
            foreach (var propertyInfo in type.GetProperties())
            {
                if(!excludedTypes.Contains(propertyInfo.PropertyType) && 
                   !excludedMembers.Contains(propertyInfo.Name))
                    sb.Append(identation + propertyInfo.Name + " = " +
                              PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }
    }
}