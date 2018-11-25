using System;
using System.Linq;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public PrintingConfig<TOwner> ExcludingType<TType>()
        {
            return this;
        }

        public PrintingConfig<TOwner> ExcludingProperty<TType>(Func<TOwner, TType> propertySelector)
        {
            return this;
        }

        public TypeSerializingContext<TOwner, TType> Serializing<TType>()
        {
            return new TypeSerializingContext<TOwner, TType>(this);
        }

        public PropertySerializingContext<TOwner, TType> Serializing<TType>(Func<TOwner, TType> propertySelector)
        {
            return new PropertySerializingContext<TOwner, TType>(this);
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

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                sb.Append(indentation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }
    }

    public static class PrintingConfigExtensions
    {
        public static string Print<TOwner>(this TOwner obj)
        {
            return ObjectPrinter.For<TOwner>().PrintToString(obj);
        }

        public static string Print<TOwner>(this TOwner obj, Func<PrintingConfig<TOwner>, PrintingConfig<TOwner>> configurator)
        {
            var printer = ObjectPrinter.For<TOwner>();
            return configurator(printer).PrintToString(obj);
        }
    }
}