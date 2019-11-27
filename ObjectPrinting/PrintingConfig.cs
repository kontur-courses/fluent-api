using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using FluentAssertions;
using FluentAssertions.Common;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private readonly HashSet<Type> excludingTypes;
        private readonly HashSet<PropertyInfo> excludingProperty;
        private readonly Dictionary<Type, Delegate> typeSerialisation;
        private readonly Dictionary<PropertyInfo, Delegate> propertySerialisation;

        Dictionary<Type, Delegate> IPrintingConfig.typeSerialisation => typeSerialisation;

        Dictionary<PropertyInfo, Delegate> IPrintingConfig.propertySerialisation => propertySerialisation;

        public PrintingConfig()
        {
            excludingTypes = new HashSet<Type>();
            excludingProperty = new HashSet<PropertyInfo>();
            typeSerialisation = new Dictionary<Type, Delegate>();
            propertySerialisation = new Dictionary<PropertyInfo, Delegate>();
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
            else
            {
                return PrintClass(obj, nestingLevel);
            }
        }

        private string PrintClass(object obj, int nestingLevel)
        {
            
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties().SkipWhile(prop => excludingTypes.Contains(prop.PropertyType) || excludingProperty.Contains(prop)))
            {
                sb.Append(identation + propertyInfo.Name + " = " + PrintToString(PrintProperty(propertyInfo,obj), nestingLevel + 1));
            }

            return sb.ToString();
        }

        private object PrintProperty(PropertyInfo propertyInfo, object obj)
        {
            var value = propertyInfo.GetValue(obj);
            if (typeSerialisation.ContainsKey(propertyInfo.PropertyType))
            {
                value = typeSerialisation[propertyInfo.PropertyType].DynamicInvoke(value);
            }

            if (propertySerialisation.ContainsKey(propertyInfo))
            {
                value = propertySerialisation[propertyInfo].DynamicInvoke(value);
            }

            return value;
        }

        public PropertySerializingConfig<TOwner, T> AlternativeFor<T>()
        {
            return new PropertySerializingConfig<TOwner, T>(this);
        }

        public PropertySerializingConfig<TOwner, T> AlternativeFor<T>(Expression<Func<TOwner, T>> func)
        {
            if (func.Body is MemberExpression memberExpression && memberExpression.Member is PropertyInfo propertyInfo)
            {
                return new PropertySerializingConfig<TOwner, T>(this, propertyInfo);
            }

            throw new ArgumentException();
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            excludingTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> func)
        {
            if (func.Body is MemberExpression memberExpression && memberExpression.Member is PropertyInfo propertyInfo)
            {
                excludingProperty.Add(propertyInfo);
            }
            else
            {
                throw new ArgumentException();
            }

            return this;
        }
    }
}