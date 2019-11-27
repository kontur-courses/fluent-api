using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        Dictionary<Type, Func<object, string>> IPrintingConfig.WaysToSerialize => waysToSerialize; 
        Dictionary<Type, CultureInfo> IPrintingConfig.TypesCultures => typesCultures;

        private readonly HashSet<Type> typesToIgnore;
        private readonly Dictionary<Type, Func<object, string>> waysToSerialize;
        private readonly Dictionary<Type, CultureInfo> typesCultures;

        public PrintingConfig()
        {
            typesToIgnore = new HashSet<Type>();
            waysToSerialize = new Dictionary<Type, Func<object, string>>();
            typesCultures = new Dictionary<Type, CultureInfo>();
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
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            typesToIgnore.Add(typeof(TPropType));
            return this;
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

            var type = obj.GetType();
            if (waysToSerialize.ContainsKey(type))
                return waysToSerialize[type](obj) + Environment.NewLine;

            if (typesCultures.ContainsKey(type))
                return string.Format(typesCultures[type], "{0}", obj) + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                var propertyValue = PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
                if (typesToIgnore.Contains(propertyInfo.PropertyType))
                    continue;
                sb.Append(indentation + propertyInfo.Name + " = " + propertyValue);
            }
            return sb.ToString();
        }
    }

    public interface IPrintingConfig
    {
        Dictionary<Type, Func<object, string>> WaysToSerialize { get; }

        Dictionary<Type, CultureInfo> TypesCultures { get; }
    }
}