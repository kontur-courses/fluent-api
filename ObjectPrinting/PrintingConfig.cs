using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using FluentAssertions.Common;
using NUnit.Framework;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly Dictionary<Type, Delegate> typePrintingFunctions = new Dictionary<Type, Delegate>();
        private readonly Dictionary<Type, CultureInfo> numberTypesToCulture = new Dictionary<Type, CultureInfo>();
        private readonly Dictionary<string, Delegate> propNamesPrintingFunctions = new Dictionary<string, Delegate>();
        private readonly Dictionary<string, int> stringPropNamesTrimming = new Dictionary<string, int>();
        private readonly HashSet<string> excludingPropertiesNames = new HashSet<string>();
        private readonly HashSet<Type> excludingTypes = new HashSet<Type>();


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
            var propName = ((MemberExpression) memberSelector.Body).Member.Name;
            excludingPropertiesNames.Add(propName);
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
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };


            var numberTypes = new[]
            {
                typeof(int), typeof(double), typeof(long)
            };
            if (finalTypes.Contains(obj.GetType()))
            {
                if (numberTypes.Contains(obj.GetType()) && numberTypesToCulture.ContainsKey(obj.GetType()))
                {
                    return ((IFormattable) obj).ToString("", numberTypesToCulture[obj.GetType()]) + Environment.NewLine;
                }

                return obj + Environment.NewLine;
            }

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);


            foreach (var propertyInfo in type.GetProperties())
            {
                var header = indentation + propertyInfo.Name + " = ";
                string content = null;
                if (excludingTypes.Contains(propertyInfo.PropertyType))
                    continue;
                if (excludingPropertiesNames.Contains(propertyInfo.Name))
                    continue;

                content = GetContent(propertyInfo, obj, nestingLevel);
                sb.Append(header + content);
            }

            return sb.ToString();
        }

        private string GetCollectionInside(object[] arr)
        {
            var elememts = new List<string>();
            foreach (var e in arr)
            {
                elememts.Add(e.ToString());
            }

            return string.Join(", ", elememts);
        }

        private string GetContent(PropertyInfo propertyInfo, object obj, int nestingLevel)
        {
            if (propertyInfo.IsIndexer())
            {
                var value = GetPropertyValue(propertyInfo, obj);
                return GetCollectionInside(value);
            }


            if (propNamesPrintingFunctions.ContainsKey(propertyInfo.Name))
            {
                return propNamesPrintingFunctions[propertyInfo.Name]
                    .DynamicInvoke(propertyInfo.GetValue(obj)).ToString();
            }

            if (typePrintingFunctions.ContainsKey(propertyInfo.PropertyType))
            {
                return typePrintingFunctions[propertyInfo.PropertyType]
                    .DynamicInvoke(propertyInfo.GetValue(obj)).ToString();
            }

            if (stringPropNamesTrimming.ContainsKey(propertyInfo.Name))
            {
                var length = propertyInfo.GetValue(obj).ToString().Length;
                return PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1)
                    .Substring(0, Math.Min(length, stringPropNamesTrimming[propertyInfo.Name]));
            }


            return PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
        }

        private object[] GetPropertyValue(PropertyInfo propertyInfo, object obj)
        {
            var length = ((IEnumerable<int>) obj).Count();
            var arr = new object[length];
            for (var i = 0; i < length; i++)
            {
                arr[i] = propertyInfo.GetValue(obj, new object[] {i});
            }

            return arr;
        }

        Dictionary<Type, Delegate> IPrintingConfig<TOwner>.TypePrintingFunctions => typePrintingFunctions;
        Dictionary<Type, CultureInfo> IPrintingConfig<TOwner>.NumberTypesToCulture => numberTypesToCulture;
        Dictionary<string, Delegate> IPrintingConfig<TOwner>.PropNamesPrintingFunctions => propNamesPrintingFunctions;
        Dictionary<string, int> IPrintingConfig<TOwner>.StringPropNamesTrimming => stringPropNamesTrimming;
        HashSet<string> IPrintingConfig<TOwner>.ExcludingPropertiesNames => excludingPropertiesNames;
        HashSet<Type> IPrintingConfig<TOwner>.ExcludingTypes => excludingTypes;
    }

    public interface IPrintingConfig<TOwner>
    {
        Dictionary<Type, Delegate> TypePrintingFunctions { get; }
        Dictionary<Type, CultureInfo> NumberTypesToCulture { get; }
        Dictionary<string, Delegate> PropNamesPrintingFunctions { get; }
        Dictionary<string, int> StringPropNamesTrimming { get; }
        HashSet<string> ExcludingPropertiesNames { get; }
        HashSet<Type> ExcludingTypes { get; }
    }
}