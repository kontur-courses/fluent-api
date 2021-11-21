using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly List<Type> finalTypes = new()
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
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

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            
            return BuildObjectString(obj, nestingLevel);
        }

        private string BuildObjectString(object obj, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var builder = new StringBuilder();
            var type = obj.GetType();
            builder.AppendLine(type.Name);

            foreach (var propertyInfo in type.GetProperties())
            {
                builder.Append(indentation
                               + propertyInfo.Name
                               + " = "
                               + PrintToString(propertyInfo.GetValue(obj),
                                   nestingLevel + 1));
            }

            return builder.ToString();
        }
    }
}