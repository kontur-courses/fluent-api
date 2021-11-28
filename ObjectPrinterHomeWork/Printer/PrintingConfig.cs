using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Printer
{
    public class PrintingConfig<TOwner>
    {
        private readonly Dictionary<Type, Func<object, string>> customSerializers = new();
        private readonly List<Type> excludedTypes = new ();

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
            foreach (var propertyInfo in type.GetProperties().Where(x => !excludedTypes.Contains(x.PropertyType)))
            {
                sb.Append(identation + GetString(propertyInfo,obj,nestingLevel));
            }

            return sb.ToString();
        }

        public PrintingConfig<TOwner> Exclude<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> StringOf<T>(Func<T, string> serializer)
        {
            customSerializers[typeof(T)] = e => serializer((T)e);
            return this;
        }

        private string GetString(PropertyInfo propertyInfo, object obj, int nestingLevel)
        {
            if (customSerializers.ContainsKey(propertyInfo.PropertyType))
                return customSerializers[propertyInfo.PropertyType](propertyInfo.GetValue(obj));

            return propertyInfo.Name + "=" + PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
            
        }
    }
    
}