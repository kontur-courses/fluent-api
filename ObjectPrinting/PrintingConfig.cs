using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

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
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberSelector.Name);
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
                return "null" ;

            var type = obj.GetType();

            if (visitedObjects.Contains(obj, ReferenceEqualityComparer.Instance))
                return "Loop found";

            if (typesToExclude.Contains(type))
                return string.Empty;

            visitedObjects.Add(obj);

            if (typePrinters.TryGetValue(type, out var printer))
                return printer(obj);

            if (FinalTypes.Set.Contains(type))
                return obj.ToString();

            if (obj is not ICollection) return PrintObj(obj, nestingLevel);
            var collection = (IEnumerable) obj;
            return PrintCollection(collection, nestingLevel);
        }

        private string PrintObj( object obj,  int nestingLevel)
        {
            var type = obj.GetType();
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);

            foreach (var memberInfo in type
                         .GetProperties()
                         .Cast<MemberInfo>()
                         .Concat(type.GetFields(BindingFlags.Instance | BindingFlags.Public)))
            {

                var name = memberInfo.Name;
                var value = memberInfo.GetValue(obj);
                if (value == null)
                {
                    sb.Append(indentation + name + " = null\n");
                    continue;
                }

                var propertyType = value.GetType();

                if (propertyPrinters.TryGetValue(name, out var propertyPrinter))
                {
                    sb.AppendLine(propertyPrinter(value));
                    continue;
                }

                if (typesToExclude.Contains(propertyType) || propertiesToExclude.Contains(name))
                    continue;

                sb.Append(indentation + name + " = ");
                sb.AppendLine(PrintToString(value, nestingLevel + 1));
            }
            return sb.ToString();
        }

        private string PrintCollection(IEnumerable collection, int nestingLevel)
        {
            if (collection is IDictionary dictionary)
                return PrintDictionary( dictionary,  nestingLevel);

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            foreach (var element in collection)
            {
                sb.Append("\n" + indentation);
                sb.Append(PrintToString(element, nestingLevel + 1));
            }
            return sb.ToString();
        }

        private string PrintDictionary(IDictionary collection, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();

            foreach (var key in collection.Keys)
            {
                sb.Append("\n" + indentation);
                sb.Append("key = ");
                sb.Append(PrintToString(key, nestingLevel + 1));
                sb.Append("\n" + indentation);
                sb.Append("value = ");
                sb.Append(PrintToString(collection[key], nestingLevel + 1));
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