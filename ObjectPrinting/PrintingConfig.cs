using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> ExcludedTypes;
        private readonly HashSet<string> ExcludedProperties;
        private readonly Dictionary<Type, Func<object, string>> SerializeFunctionsForTypes;
        private readonly Dictionary<string, Func<object, string>> SerializeFunctionsForProperties;
        private readonly HashSet<object> AlreadySerialized;
        private readonly HashSet<Type> FinalTypes;

        public PrintingConfig()
        {
            ExcludedTypes = new HashSet<Type>();
            SerializeFunctionsForTypes = new Dictionary<Type, Func<object, string>>();
            SerializeFunctionsForProperties = new Dictionary<string, Func<object, string>>();
            ExcludedProperties = new HashSet<string>();
            AlreadySerialized = new HashSet<object>();
            FinalTypes = new HashSet<Type>
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberSelector);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body is MemberExpression)
            {
                var memberExpr = memberSelector.Body as MemberExpression;
                var propertyName = memberExpr.Member.Name;
                ExcludedProperties.Add(propertyName);
            }
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            ExcludedTypes.Add(typeof(TPropType));
            return this;
        }

        public void SetSerialization<TPropType>(Func<TPropType, string> print)
        {
            SerializeFunctionsForTypes.Add(typeof(TPropType), x => print((TPropType)x));
        }

        public void SetSerialization<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector, Func<TPropType, string> print)
        {
            if (memberSelector.Body is MemberExpression)
            {
                var memberExpr = memberSelector.Body as MemberExpression;
                var propertyName = memberExpr.Member.Name;
                Func<object, string> func = x => print((TPropType) x);
                SerializeFunctionsForProperties.Add(propertyName, func);
            }
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

            AlreadySerialized.Add(obj);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            if (obj is IEnumerable)
                sb.Append(PrintIEnumerableToString((IEnumerable)obj, nestingLevel));
            else
                type
                    .GetProperties()
                    .Where(x=> !ExcludedTypes.Contains(x.PropertyType))
                    .Where(x=> !ExcludedProperties.Contains(x.Name))
                    .Where(x=> !AlreadySerialized.Contains(x.GetValue(obj)))
                    .ToList()
                    .ForEach(x => sb.Append(PrintPropertyToString(x, obj, nestingLevel)));
            return sb.ToString();
        }

        private string PrintIEnumerableToString(IEnumerable obj, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            obj
                .Cast<object>()
                .ToList()
                .ForEach(x=>sb.Append(identation + PrintToString(x, nestingLevel + 1)));
            return sb.ToString();
        }

        private string PrintPropertyToString(PropertyInfo propertyInfo, object obj, int nestingLevel)
        {
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            if (SerializeFunctionsForProperties.ContainsKey(propertyInfo.Name))
                sb.Append(PrintPropertyWithCustomSerializator(propertyInfo, obj, identation, 
                    SerializeFunctionsForProperties[propertyInfo.Name]));
            else if (SerializeFunctionsForTypes.ContainsKey(propertyInfo.PropertyType))
                sb.Append(PrintPropertyWithCustomSerializator(propertyInfo, obj, identation,
                    SerializeFunctionsForTypes[propertyInfo.PropertyType]));
            else
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            return sb.ToString();
        }

        private string PrintPropertyWithCustomSerializator(PropertyInfo propertyInfo, object obj, string identation, Func<object, string> serializator)
        {
            return identation + propertyInfo.Name + " = " +
                   serializator.Invoke(propertyInfo.GetValue(obj))
                   + Environment.NewLine;
        }
    }
}