using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    
    public class PrintingConfig<TOwner>
    {
        private List<Type> removedTypes = new List<Type>();
        private Dictionary<Type, CultureInfo> cultures = new Dictionary<Type, CultureInfo>();

        private Dictionary<Type, Func<object, string>> alternativeSerialization = 
            new Dictionary<Type, Func<object, string>>();
        
        //тут бы название свойства
        public Dictionary<String, Func<object, string>> propertySerialization = 
            new Dictionary<String, Func<object, string>>();
        
        //и тут бы тоже название свойства
        // public Dictionary<MemberInfo, Func<object, string>> crop = 
        //     new Dictionary<String, Func<object, string>>();
        
        private List<PropertyInfo> exludingProperty = new List<PropertyInfo>();
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public PrintingConfig<TOwner> ExcludeType<T>()
        {
            var tType = typeof(T);
            removedTypes.Add(tType);
            return this;
        }

        public PrintingConfig<TOwner> ExcludeProperty(PropertyInfo propertyInfo)
        {
            exludingProperty.Add(propertyInfo);
            return this;
        }
            

        public PrintingConfig<TOwner> SerializeType<T>(Func<T, string> func)
        {
            alternativeSerialization.Add(typeof(T), x => func((T)x));
            return this;
        }

        public PrintingConfig<TOwner> SetCulture<T>(CultureInfo culture)
        {
            cultures.Add(typeof(T), culture);
            return this;
        }
        
        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;
            var type = obj.GetType();
            
            if (alternativeSerialization.ContainsKey(type))
                return alternativeSerialization[type](obj) + Environment.NewLine;
            
            if (cultures.ContainsKey(type))
                return ((IFormattable)obj).ToString(null, cultures[type]) + Environment.NewLine;
            
            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            
            if (finalTypes.Contains(type))
            {
                return obj + Environment.NewLine;
            }


            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            

            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (removedTypes.Contains(propertyInfo.PropertyType))
                    continue;
                
                if (exludingProperty.Contains(propertyInfo))
                    continue;

                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }


            return sb.ToString();
        }
    }
}
