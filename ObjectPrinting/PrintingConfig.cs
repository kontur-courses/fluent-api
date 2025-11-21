using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private HashSet<Type> excludingTypes = new();
        private HashSet<string> excludingProperties = new();
        private readonly Dictionary<Type, Func<object, string>> typeSerializers = new();

        public PrintingConfig<TOwner> Exclude<T>()
        {
            excludingTypes.Add(typeof(T));
            return this;
        }
        
        public PrintingConfig<TOwner> Exclude<TProp>(Expression<Func<TOwner, TProp>> propertySelector)
        {
            var propertyName = ((MemberExpression)propertySelector.Body).Member.Name;
            excludingProperties.Add(propertyName);
            return this;
        }

        public TypePrintingConfig<TOwner, TPropType> ConfigureType<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        internal void AddTypeSerializer<TPropType>(Func<TPropType, string> serializer)
        {
            typeSerializers[typeof(TPropType)] = obj => serializer((TPropType)obj);
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var objectType = obj.GetType();

            if (typeSerializers.TryGetValue(objectType, out var serializer))
                return serializer(obj) + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan), typeof(Guid)
            };
            
            if (finalTypes.Contains(objectType))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(objectType.Name);
            
            foreach (var propertyInfo in objectType.GetProperties())
            {
                var propertyName = propertyInfo.Name;
                var propertyType = propertyInfo.PropertyType;
        
                if (excludingProperties.Contains(propertyName) || excludingTypes.Contains(propertyType))
                    continue;

                var value = propertyInfo.GetValue(obj);
                sb.Append(identation + propertyName + " = " + PrintToString(value, nestingLevel + 1));
            }
            return sb.ToString();
        }
    }
}