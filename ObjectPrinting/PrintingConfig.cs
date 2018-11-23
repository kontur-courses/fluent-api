using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            return this;
        }

        public TypePrintingConfig<TOwner, TPropType> Serializing<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public TypePrintingConfig<TOwner, TPropType> Serializing<TPropType>(Expression<Func<TOwner, TPropType>> propertyFunc)
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public TypePrintingConfig<TOwner, string> Serializing<String>(Expression<Func<TOwner, string>> propertyFunc)
        {
            return new TypePrintingConfig<TOwner, string>(this);
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
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }
    }
}