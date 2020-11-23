using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.PrintingConfig
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes;
        private readonly HashSet<string> excludedProperties;

        public PrintingConfig()
        {
            excludedTypes = new HashSet<Type>();
            excludedProperties = new HashSet<string>();
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
                typeof(DateTime), typeof(TimeSpan), typeof(Guid)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedTypes.Contains(propertyInfo.PropertyType)
                    || excludedProperties.Contains(propertyInfo.Name)) continue;
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }
        
        public PrintingConfig<TOwner> ExcludingProperty<TProp>(Expression<Func<TOwner, TProp>> memberSelector)
        {
            var propertyInfo =
                ((MemberExpression) memberSelector.Body).Member as PropertyInfo;
            excludedProperties.Add(propertyInfo?.Name);
            return this;
        }
        
        public PrintingConfig<TOwner> ExcludingPropertyWithType<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }
        
        public PrintingConfig<TOwner> ExcludingPropertyWithTypes(params Type[] types)
        {
            excludedTypes.UnionWith(types);
            return this;
        }
    }
}