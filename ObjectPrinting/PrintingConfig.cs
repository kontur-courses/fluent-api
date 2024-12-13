using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NUnit.Framework.Internal;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> typesToExclude = [];
        private readonly HashSet<string> propertiesToExclude = [];
        private readonly Dictionary<Type, Func<object, string>> typePrinters = [];
        private readonly Dictionary<string, Func<object, string>> propertyPrinters = [];
        private readonly HashSet<object> visitedObjects = [];

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body is not MemberExpression memberExpression)
                throw new ArgumentException("Wrong Expression");
            propertiesToExclude.Add(memberExpression.Member.Name);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            typesToExclude.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }
        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return string.Empty;

            var type = obj.GetType();

            if (visitedObjects.Contains(obj))
                return "Loop found";

            if (typesToExclude.Contains(type))
                return string.Empty;

            visitedObjects.Add(obj);

            if (obj is ICollection)
            {
                var collection = (IEnumerable) obj;
                return PrintCollection(collection, nestingLevel);
            }


            if (typePrinters.TryGetValue(type, out var serializer))
                return serializer(obj);

            if (FinalTypes.Set.Contains(type))
                return obj.ToString();

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);

            foreach (var propertyInfo in type.GetProperties())
            {
                var propertyType = propertyInfo.PropertyType;
                var propertyName = propertyInfo.Name;
                var propertyValue = propertyInfo.GetValue(obj);

                if (propertyPrinters.TryGetValue(propertyName, out var propertySerializer))
                {
                    _ = sb.AppendLine(propertySerializer(propertyValue));
                    continue;
                }

                if (typesToExclude.Contains(propertyType) || propertiesToExclude.Contains(propertyName))
                    continue;

                sb.Append(indentation + propertyName + " = ");
                _ = sb.AppendLine(PrintToString(propertyValue, nestingLevel + 1));
            }
            return sb.ToString();
        }

        private string PrintCollection(IEnumerable collection, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            foreach (var element in collection)
            {
                sb.Append("\n");
                sb.Append(indentation);
                _ = sb.Append(PrintToString(element, nestingLevel + 1));
            }
            return sb.ToString();
        }

        public void AddTypePrinter<TPropType>(Func<TPropType, string> print)
        {
            typePrinters[typeof(TPropType)] = obj => print((TPropType)obj);
        }

        internal void AddPropertyPrinter(string propName, Func<object, string> print)
        {
            propertyPrinters[propName] = print;
        }
    }
}