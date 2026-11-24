using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Globalization;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly PrintingSettings settings = new();

        private static readonly HashSet<Type> finalTypes = new()
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan),
            typeof(decimal), typeof(bool), typeof(char), typeof(Guid),
            typeof(byte), typeof(sbyte), typeof(short), typeof(ushort),
            typeof(uint), typeof(long), typeof(ulong)
        };

        public string PrintToString(TOwner obj)
        {
            var visitedObjects = new HashSet<object> (ReferenceEqualityComparer.Instance);
            return PrintToString(obj, 0, visitedObjects);
        }

        public PrintingConfig<TOwner> ExcludingType<T>()
        {
            settings.ExcludeType(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TProp>(Expression<Func<TOwner,TProp>> selector)
        {
            var memberExpression = selector.Body as MemberExpression;

            if (memberExpression == null)
                throw new ArgumentException("Incorrect access to the property.A delegate of the form p => p.Age is required.");

            settings.ExcludeMember(memberExpression.Member);

            return this;
        }

        public PrintingConfig<TOwner> PrintTypeAs<T>(Func<T, string> printer)
        {
            settings.SetTypePrinter(typeof(T), obj => printer((T)obj));
            return this;
        }
        
        public PrintingConfig<TOwner> WithCulture<TProp>(CultureInfo culture) where TProp : IFormattable
        {
            settings.SetCulture(typeof(TProp), culture);
            return this;
        }

        public PropertyPrintingConfig<TOwner, TProp> Printing<TProp>(Expression<Func<TOwner,TProp>> selector)
        {
            var expression = selector.Body as MemberExpression;

            if (expression == null)
                throw new ArgumentException("Incorrect access to the property. A delegate of the form p => p.Age is required.");

            return new PropertyPrintingConfig<TOwner, TProp>(this, expression.Member);
        }

        internal void SetMemberPrinter(MemberInfo member, Func<object, string> printer)
        {
            settings.SetMemberPrinter(member, printer);
        }

        private string PrintToString(object obj, int nestingLevel, HashSet<object> visitedObjects)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (settings.TryGetTypePrinter(obj.GetType(), out var printer)) //проверка PrintTypeAs
                return printer(obj) + Environment.NewLine;

            if (settings.TryGetCulture(obj.GetType(), out var culture) //проверка культуры
                && obj is IFormattable formattable) 
                return formattable.ToString(null, culture) + Environment.NewLine;

            
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            if (visitedObjects.Contains(obj))
                return "cycle reference" + Environment.NewLine; //проверка рекурсивности

            visitedObjects.Add(obj);

            if (obj is IDictionary dictionary)
                return PrintDictionary(dictionary, nestingLevel, visitedObjects);

            if (obj is IEnumerable enumerable)
                return PrintCollection(enumerable, nestingLevel, visitedObjects);

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (settings.IsTypeExcluded(propertyInfo.PropertyType)) //проверка исключенного типа
                    continue;

                if (settings.IsMemberExcluded(propertyInfo)) //проверка исключенного свойства
                     continue;

                if (settings.TryGetMemberPrinter(propertyInfo, out var memberPrinter))
                {
                    sb.Append(identation + propertyInfo.Name + " = " +
                              memberPrinter(propertyInfo.GetValue(obj)) + Environment.NewLine);
                    continue;
                }

                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1, visitedObjects));
            }
            return sb.ToString();
        }

        private string PrintCollection(IEnumerable collection, int nestingLevel, HashSet<object> visitedObjects)
        {
            var sb = new StringBuilder();
            var indent = new string('\t', nestingLevel + 1);

            sb.AppendLine(collection.GetType().Name);

            int index = 0;
            foreach (var item in collection)
            {
                sb.Append(indent + $"[{index}] = " +
                          PrintToString(item, nestingLevel + 1, visitedObjects));
                index++;
            }

            return sb.ToString();
        }

        private string PrintDictionary(IDictionary dictionary, int nestingLevel, HashSet<object> visitedObjects)
        {
            var sb = new StringBuilder();
            var indent = new string('\t', nestingLevel + 1);

            sb.AppendLine(dictionary.GetType().Name);

            foreach (DictionaryEntry entry in dictionary)
            {
                var keyStr = PrintToString(entry.Key, 0, visitedObjects).TrimEnd('\r', '\n');
                sb.Append(indent + $"[{keyStr}] = " +
                          PrintToString(entry.Value, nestingLevel + 1, visitedObjects));
            }

            return sb.ToString();
        }
    }
}