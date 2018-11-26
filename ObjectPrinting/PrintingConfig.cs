using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Tests;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public List<Func<PropertyInfo, bool>> ExcludeProperties;

        public PrintingConfig()
        {
            ExcludeProperties = new List<Func<PropertyInfo, bool>>();
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

            var properties = type.GetProperties();
            foreach (var e in ExcludeProperties)
                properties = properties.Where(e).ToArray();

            sb.AppendLine(type.Name);
            foreach (var propertyInfo in properties)
            {
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }

        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            var excludedType = typeof(TPropType);
            var excludedFunc = new Func<PropertyInfo, bool>(property => property.PropertyType.FullName != excludedType.FullName);
            ExcludeProperties.Add(excludedFunc);
            return this;
        }

        public PrintingConfig<TOwner> Exclude(string excludedNameProp)
        {
            var excludedFunc = new Func<PropertyInfo, bool>(property => property.Name != excludedNameProp);
            ExcludeProperties.Add(excludedFunc);
            return this;
        }

        public TypePrintingConfig<TOwner, TPropType> Serializer<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }


        public TypePrintingConfig<TOwner, TPropType> Serializer<TPropType>(Expression<Func<TOwner, TPropType>> propertyFunc)
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }
    }
}