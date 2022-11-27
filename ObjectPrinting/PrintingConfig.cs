using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> FinalTypes = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };
        private HashSet<Type> excludingTypes = new HashSet<Type>();

        public PropertyConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyConfig<TOwner, TPropType>(this);
        }

        public PropertyConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyConfig<TOwner, TPropType>(this);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludingTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            if (FinalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;
            var identation = new string('\t', nestingLevel + 1);
            var stringBuilder = new StringBuilder();
            var type = obj.GetType();
            stringBuilder.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if(excludingTypes.Contains(propertyInfo.PropertyType))
                    break;
                stringBuilder.Append(identation);
                stringBuilder.Append(propertyInfo.Name);
                stringBuilder.Append(" = ");
                stringBuilder.Append(PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1));
            }

            return stringBuilder.ToString();
        }
    }
}