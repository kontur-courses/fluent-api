using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly HashSet<Type> notSerialisedTypeOfProperty = new HashSet<Type>();
        private readonly HashSet<string> notSerialisedProperty = new HashSet<string>();

        private readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private readonly Dictionary<Type, Func<object, string>> serialised =
            new Dictionary<Type, Func<object, string>>();

        private readonly Dictionary<string, Func<object, string>> serialisedProperty =
            new Dictionary<string, Func<object, string>>();

        private readonly Dictionary<Type, Func<object, string>> cultureTypes =
            new Dictionary<Type, Func<object, string>>();

        private readonly Dictionary<string, Func<string, string>> trimmer =
            new Dictionary<string, Func<string, string>>();

        private readonly List<Func<string, string>> trim = new List<Func<string, string>> {s => s};

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
            var propInfo =
                ((MemberExpression) memberSelector.Body).Member as PropertyInfo;
            notSerialisedProperty.Add(propInfo.Name);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            notSerialisedTypeOfProperty.Add(typeof(TPropType));
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

            var type = obj.GetType();
            if (notSerialisedTypeOfProperty.Contains(type))
                return null;

            if (type == typeof(string))
            {
                if (serialised.ContainsKey(type))
                    return trim[0](serialised[type](obj)) + Environment.NewLine;
                return trim[0]((string) obj) + Environment.NewLine;
            }

            if (serialised.ContainsKey(type))
            {
                return serialised[type](obj) + Environment.NewLine;
            }

            if (cultureTypes.ContainsKey(type))
                return cultureTypes[type](obj) + Environment.NewLine;
            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (notSerialisedTypeOfProperty.Contains(propertyInfo.PropertyType) ||
                    notSerialisedProperty.Contains(propertyInfo.Name))
                    continue;
                if (trimmer.ContainsKey(propertyInfo.Name))
                {
                    if (serialisedProperty.ContainsKey(propertyInfo.Name))
                        sb.Append(indentation + propertyInfo.Name + " = " +
                                  trimmer[propertyInfo.Name](
                                      serialisedProperty[propertyInfo.Name](propertyInfo.GetValue(obj))) +
                                  Environment.NewLine);
                    else
                        sb.Append(indentation + propertyInfo.Name + " = " +
                                  trimmer[propertyInfo.Name](
                                      (string) propertyInfo.GetValue(obj)) +
                                  Environment.NewLine);
                    continue;
                }

                if (serialisedProperty.ContainsKey(propertyInfo.Name))
                {
                    sb.Append(indentation + propertyInfo.Name + " = " +
                              serialisedProperty[propertyInfo.Name](propertyInfo.GetValue(obj)) + Environment.NewLine);
                    continue;
                }

                sb.Append(indentation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }

            return sb.ToString();
        }

        Dictionary<Type, Func<object, string>> IPrintingConfig<TOwner>.Serialised => serialised;
        Dictionary<string, Func<object, string>> IPrintingConfig<TOwner>.SerialisedProperty => serialisedProperty;
        Dictionary<Type, Func<object, string>> IPrintingConfig<TOwner>.CultureTypes => cultureTypes;
        Dictionary<string, Func<string, string>> IPrintingConfig<TOwner>.Trimmer => trimmer;
    }
}