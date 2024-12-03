using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludeType= new ();
        private readonly Dictionary<Type, Func<string, string, string>> serializer = [];
        private CultureInfo culture = CultureInfo.CurrentCulture;
       

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
                typeof(DateTime), typeof(TimeSpan), typeof(Guid)
            };
            if (finalTypes.Contains(obj.GetType()))
            {
                
                return obj.GetType() + Environment.NewLine;
            }
            
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludeType.Contains(propertyInfo.PropertyType)) continue;
                
                if (!serializer.ContainsKey(propertyInfo.PropertyType))
                {
                    sb.Append(identation + propertyInfo.Name + " = " +
                              PrintToString(propertyInfo.GetValue(obj),
                                  nestingLevel + 1));
                    continue;
                }
                sb.Append(identation + serializer[propertyInfo.PropertyType](propertyInfo.Name,
                    propertyInfo.GetValue(obj) + Environment.NewLine));
            }
            return sb.ToString();
        }
        
        public PrintingConfig<TOwner> Exclude<T>()
        {
            excludeType.Add(typeof(T));
            
            return this;
        }

        public PrintingConfig<TOwner> SerializeFor<TProperty>(Func<string, string, string> func)
        {
            serializer.Add(typeof(TProperty), func);
            return this;
        }

        public PrintingConfig<TOwner> UseCulture(CultureInfo cultureInfo)
        {
            
        }
    }
}