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
        private readonly HashSet<object> alreadySerialized;
        private readonly HashSet<string> excludedProperties;
        private readonly HashSet<Type> excludedTypes;
        private readonly HashSet<Type> finalTypes;
        private readonly Dictionary<string, Func<object, string>> serializeFunctionsForProperties;
        private readonly Dictionary<Type, Func<object, string>> serializeFunctionsForTypes;

        public PrintingConfig()
        {
            excludedTypes = new HashSet<Type>();
            serializeFunctionsForTypes = new Dictionary<Type, Func<object, string>>();
            serializeFunctionsForProperties = new Dictionary<string, Func<object, string>>();
            excludedProperties = new HashSet<string>();
            alreadySerialized = new HashSet<object>();
            finalTypes = new HashSet<Type>
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan), typeof(sbyte), typeof(ushort),
                typeof(short), typeof(uint), typeof(long), typeof(ulong)
            };
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberSelector);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body is MemberExpression memberExpr)
            {
                var propertyName = memberExpr.Member.Name;
                excludedProperties.Add(propertyName);
            }

            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public void SetSerialization<TPropType>(Func<TPropType, string> print)
        {
            serializeFunctionsForTypes.Add(typeof(TPropType), x => print((TPropType) x));
        }

        public void SetSerialization<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector,
            Func<TPropType, string> print)
        {
            if (!(memberSelector.Body is MemberExpression memberExpr))
                return;

            var propertyName = memberExpr.Member.Name;

            string Func(object x)
            {
                return print((TPropType) x);
            }

            serializeFunctionsForProperties.Add(propertyName, Func);
        }


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

            alreadySerialized.Add(obj);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            if (obj is IEnumerable enumerable)
                sb.Append(PrintIEnumerableToString(enumerable, nestingLevel));
            else
                type
                    .GetProperties()
                    .Where(x => !excludedTypes.Contains(x.PropertyType))
                    .Where(x => !excludedProperties.Contains(x.Name))
                    .Where(x => !alreadySerialized.Contains(x.GetValue(obj)))
                    .ToList()
                    .ForEach(x => sb.Append(PrintPropertyToString(x, obj, nestingLevel)));
            return sb.ToString();
        }

        private string PrintIEnumerableToString(IEnumerable obj, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            obj
                .Cast<object>()
                .ToList()
                .ForEach(x =>
                {
                    sb.Append(indentation);
                    sb.Append(PrintToString(x, nestingLevel + 1));
                });
            return sb.ToString();
        }

        private string PrintPropertyToString(PropertyInfo propertyInfo, object obj, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            if (serializeFunctionsForProperties.ContainsKey(propertyInfo.Name))
                return PrintPropertyWithCustomSerializator(propertyInfo, obj, indentation,
                    serializeFunctionsForProperties[propertyInfo.Name]);

            if (serializeFunctionsForTypes.ContainsKey(propertyInfo.PropertyType))
                return PrintPropertyWithCustomSerializator(propertyInfo, obj, indentation,
                    serializeFunctionsForTypes[propertyInfo.PropertyType]);

            return string.Join("", indentation, propertyInfo.Name, " = ",
                PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1));
        }

        private string PrintPropertyWithCustomSerializator(PropertyInfo propertyInfo, object obj, string indentation,
            Func<object, string> serializator)
        {
            return string.Join("", indentation, propertyInfo.Name, " = ",
                serializator.Invoke(propertyInfo.GetValue(obj)), Environment.NewLine);
        }
    }
}