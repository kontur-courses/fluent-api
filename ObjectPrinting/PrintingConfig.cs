using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes;
        private readonly HashSet<PropertyInfo> excludedProperties;
        private readonly Dictionary<Type, Delegate> customTypeSerializers;
        private readonly Dictionary<PropertyInfo, Delegate> customPropertySerializers;

        internal Dictionary<Type, Delegate> CustomTypeSerializers => customTypeSerializers;
        internal Dictionary<PropertyInfo, Delegate> CustomPropertySerializers => customPropertySerializers;

        public PrintingConfig()
        {
            excludedTypes = new HashSet<Type>();
            excludedProperties = new HashSet<PropertyInfo>();
            customTypeSerializers = new Dictionary<Type, Delegate>();
            customPropertySerializers = new Dictionary<PropertyInfo, Delegate>();
        }

        public PropertyPrintingConfig<TOwner, TPropType> SetPrintingFor<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> SetPrintingFor<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var expression = (MemberExpression)memberSelector.Body;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, (PropertyInfo)expression.Member);
        }

        public PrintingConfig<TOwner> ExcludeProperty<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var expression = (MemberExpression)memberSelector.Body;
            excludedProperties.Add((PropertyInfo)expression.Member);
            return this;
        }

        public PrintingConfig<TOwner> ExcludePropertyType<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
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
                if (excludedTypes.Contains(propertyInfo.PropertyType) || excludedProperties.Contains(propertyInfo))
                    continue;
                
                if (customTypeSerializers.TryGetValue(propertyInfo.PropertyType, out var typeSerializer))
                    sb.Append(identation + propertyInfo.Name + " = " +
                              typeSerializer.DynamicInvoke(propertyInfo.GetValue(obj)) + Environment.NewLine);
                else if (customPropertySerializers.TryGetValue(propertyInfo, out var propertySerializer))
                    sb.Append(identation + propertyInfo.Name + " = " +
                              propertySerializer.DynamicInvoke(propertyInfo.GetValue(obj)) + Environment.NewLine);
                else
                    sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }
    }
}