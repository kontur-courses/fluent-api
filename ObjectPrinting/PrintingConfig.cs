using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using ObjectPrinting.Tests;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private HashSet<Type> excludedTypes = new HashSet<Type>();

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
            foreach (var propertyInfo in GetProperties(type))
            {
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }

        IEnumerable<PropertyInfo> GetProperties(Type objType)
        {
            foreach (var propertyInfo in objType.GetProperties())
            {
                if(excludedTypes.Contains(propertyInfo.PropertyType))
                    continue;
                yield return propertyInfo;
            }
        }

        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));

            return this;
        }

        public TypePrintingConfig<TOwner, TPropType> SetAltSerialize<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }


        public TypePrintingConfig<TOwner, TPropType> SetAltSerialize<TPropType>(Expression<Func<TOwner, TPropType>> propertyFunc)
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> propertyFunc)
        {
            return this;
        }
    }


}