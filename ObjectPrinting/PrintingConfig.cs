using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public PrintingConfig<TOwner> Excluding<TType>()
        {
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TType>(Expression<Func<TOwner, TType>> selector)
        {
            return this;
        }

        public PropertyPrintingConfig<TOwner, TType> Printing<TType>()
        {
            return new PropertyPrintingConfig<TOwner, TType>(this);
        }

        public PropertyPrintingConfig<TOwner, TType> Printing<TType>(Expression<Func<TOwner, TType>> selector)
        {
            if (selector.Body is not MemberExpression expression)
                throw new ArgumentException();

            return new PropertyPrintingConfig<TOwner, TType>(this, expression.Member);
        }

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

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }
    }
}