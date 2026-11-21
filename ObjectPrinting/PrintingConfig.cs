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
        private readonly HashSet<Type> excludedTypes = [];
        private readonly HashSet<PropertyInfo> excludedProperties = [];

        private readonly Dictionary<Type, Func<object, string>> formattersByType = new();
        private readonly Dictionary<PropertyInfo, Func<object, string>> formattersByProperty = new();
        
        
        
        
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, GetProperty(memberSelector));
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludedProperties.Add(GetProperty(memberSelector));
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
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
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }


        private static PropertyInfo GetProperty<TPropType>(Expression<Func<TPropType>> selector)
        {
            //метод должен на основе селектора(например p => p.Name) вернуть PropertyInfo, описывающее свойство (напр Person.Name)
            
            
            throw new NotImplementedException();
        }

        internal void SetFormatter(Type type, PropertyInfo? property, Func<object, string> formatter)
        {
            if (property == null) formattersByType[type] = formatter;
            else formattersByProperty[property] = formatter;
        }
    }
}