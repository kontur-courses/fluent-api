using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NUnit.Framework.Internal;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private List<Type> _excludedTypes = new();
        private Dictionary<Type, Delegate> _typeConverters = new();
        internal CultureInfo DoubleCultureInfo { get; set; } = CultureInfo.CurrentCulture;
        internal CultureInfo FloatCultureInfo { get; set; } = CultureInfo.CurrentCulture;

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        internal void AddConverter<TParam>(Type type, Func<TParam, string?> converter)
        {
            _typeConverters.Add(type, converter);
        }

        public PrintingConfig<TOwner> ExceptType<T>()
        {
            _excludedTypes.Add(typeof(T));
            return this;
        }

        public ITypeSerializer<TParam, TOwner> ForType<TParam>()
        {
            return new TypeSerializerImpl<TParam, TOwner>(this);
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