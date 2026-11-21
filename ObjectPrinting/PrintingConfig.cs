using FluentAssertions;
using System;
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
        private readonly HashSet<Type> excludedTypes = new();
        private readonly HashSet<MemberInfo> excludedMembers = new();
        private readonly Dictionary<Type,Func<object, string>> customPrinters = new();
        private readonly Dictionary<Type, CultureInfo> customCultures = new();
        private readonly Dictionary<MemberInfo, Func<object, string>> memberPrinters = new();
        //private HashSet<object> visitedObjects = new();
        public string PrintToString(TOwner obj)
        {
            var visitedObjects = new HashSet<object> (ReferenceEqualityComparer.Instance);
            return PrintToString(obj, 0, visitedObjects);
        }

        public PrintingConfig<TOwner> ExcludingType<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TProp>(Expression<Func<TOwner,TProp>> selector)
        {
            var memberExpression = selector.Body as MemberExpression;

            if (memberExpression == null)
                throw new ArgumentException("Неправильное обращение к свойству. Необходим делегат вида p => p.Age");

            excludedMembers.Add(memberExpression.Member);

            return this;
        }

        public PrintingConfig<TOwner> PrintTypeAs<T>(Func<T, string> printer)
        {
            customPrinters[typeof(T)] = obj => printer((T)obj);
            return this;
        }
        
        public PrintingConfig<TOwner> WithCulture<TProp>(CultureInfo culture) where TProp : IFormattable
        {
            customCultures[typeof(TProp)] = culture;
            return this;
        }

        public PropertyPrintingConfig<TOwner, TProp> Printing<TProp>(Expression<Func<TOwner,TProp>> selector)
        {
            var expression = selector.Body as MemberExpression;

            if (expression == null)
                throw new ArgumentException("Неправильное обращение к свойству.");

            return new PropertyPrintingConfig<TOwner, TProp>(this, expression.Member);
        }

        internal void SetMemberPrinter(MemberInfo member, Func<object, string> printer)
        {
            memberPrinters[member] = printer;
        }

        private string PrintToString(object obj, int nestingLevel, HashSet<object> visitedObjects)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            if (customPrinters.TryGetValue(obj.GetType(), out var printer)) //проверка PrintTypeAs
                return printer(obj) + Environment.NewLine;

            if (customCultures.TryGetValue(obj.GetType(), out var culture) //проверка культуры
                && obj is IFormattable formattable) 
                return formattable.ToString(null, culture) + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            if (visitedObjects.Contains(obj))
                return "cycle reference" + Environment.NewLine; //проверка рекурсивности

            visitedObjects.Add(obj);

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedTypes.Contains(propertyInfo.PropertyType)) //проверка исключенного типа
                    continue;

                if (excludedMembers.Contains(propertyInfo)) //проверка исключенного свойства
                     continue;

                if (memberPrinters.TryGetValue(propertyInfo, out var memberPrinter))
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

    }
}