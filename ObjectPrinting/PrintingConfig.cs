using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private static readonly HashSet<Type> FinalTypes = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(long), typeof(short), typeof(sbyte), typeof(byte),
            typeof(uint), typeof(ulong), typeof(ushort), typeof(decimal),
            typeof(DateTime), typeof(TimeSpan), typeof(char), typeof(bool)
        };

        private readonly HashSet<Type> excludingTypes;
        private readonly HashSet<string> excludingProperties;
        private readonly Dictionary<Type, Delegate> alternativeSerializersForTypes;
        private readonly Dictionary<string, Delegate> alternativeSerializersForProperties;
        Dictionary<Type, Delegate> IPrintingConfig.AlternativeSerializersForTypes => alternativeSerializersForTypes;
        Dictionary<string, Delegate> IPrintingConfig.AlternativeSerializersForProperties => alternativeSerializersForProperties;

        public PrintingConfig()
        {
            excludingTypes = new HashSet<Type>();
            excludingProperties = new HashSet<string>();
            alternativeSerializersForTypes = new Dictionary<Type, Delegate>();
            alternativeSerializersForProperties = new Dictionary<string, Delegate>();
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body is MemberExpression memberExpression)
                return new PropertyPrintingConfig<TOwner, TPropType>(this, memberExpression.Member.Name);
            throw new ArgumentException("No MemberExpression in argument");
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body is MemberExpression memberExpression)
                excludingProperties.Add(memberExpression.Member.Name);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludingTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (FinalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludingProperties.Contains(propertyInfo.Name))
                    continue;
                if (excludingTypes.Contains(propertyInfo.PropertyType))
                    continue;
                sb.Append(indentation);
                sb.Append(GetPropertySerialization(obj, propertyInfo, nestingLevel));
            }
            return sb.ToString();
        }

        private string GetPropertySerialization(
            object obj, PropertyInfo propertyInfo, int nestingLevel)
        {
            var serialization = propertyInfo.GetValue(obj);
            Delegate alternativeSerializer = null;
            if (alternativeSerializersForProperties.ContainsKey(propertyInfo.Name))
                alternativeSerializer = alternativeSerializersForProperties[propertyInfo.Name];
            else if (alternativeSerializersForTypes.ContainsKey(propertyInfo.PropertyType))
                alternativeSerializer = alternativeSerializersForTypes[propertyInfo.PropertyType];
            if (alternativeSerializer != null)
                serialization = (string)alternativeSerializer.DynamicInvoke(propertyInfo.GetValue(obj));
            return propertyInfo.Name + " = " + PrintToString(serialization, nestingLevel + 1);
        }
    }
}