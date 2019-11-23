using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting.Configs
{
    public class PrintingConfig<TOwner>
    {
        private HashSet<Type> excludedTypes = new HashSet<Type>();
        private Dictionary<Type, Func<Type, string>> alternativeSerializations = new Dictionary<Type, Func<Type, string>>();
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
            if (excludedTypes.Contains(obj.GetType()))
                return string.Empty;
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if(excludedTypes.Contains(propertyInfo.PropertyType)) continue;
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }
        
        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> propertyDelegate)
        {
            return this;
        }

        public PropertySerializationConfig<TOwner, TPropType> Serialize<TPropType>()
        {
            return new PropertySerializationConfig<TOwner, TPropType>(this);
        }
        
        public PropertySerializationConfig<TOwner, TPropType> Serialize<TPropType>(Func<TOwner, TPropType> propertyDelegate)
        {
            return new PropertySerializationConfig<TOwner, TPropType>(this);
        }
    }
}