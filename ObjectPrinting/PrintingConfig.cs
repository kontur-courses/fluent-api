using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using FluentAssertions.Formatting;
using NUnit.Framework;
using ObjectPrinting.Tests;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private Type[] finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private HashSet<Type> excludedTypes = new HashSet<Type>();
        private Dictionary<Type, Delegate> typeSerializations = new Dictionary<Type, Delegate>();
        private Dictionary<Type, CultureInfo> numberCultures = new Dictionary<Type, CultureInfo>();

        internal void AddTypeSerialization<TPropType>(Delegate func) => typeSerializations[typeof(TPropType)] = func;
        internal void SetNumberCulture<TPropType>(CultureInfo culture) => numberCultures[typeof(TPropType)] = culture;

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in GetProperties(type))
            {
                string strValue;

                if (typeSerializations.ContainsKey(propertyInfo.PropertyType))
                {
                    var tDelegate = typeSerializations[propertyInfo.PropertyType];
                    strValue = (string)tDelegate.DynamicInvoke(propertyInfo.GetValue(obj));
                }
                else if (numberCultures.ContainsKey(propertyInfo.PropertyType))
                {
                    strValue = ((IFormattable)propertyInfo.GetValue(obj)).ToString(null, numberCultures[propertyInfo.PropertyType]);
                }
                else
                {
                    strValue = PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
                }

                sb.Append(identation + propertyInfo.Name + " = " + strValue);
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