using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        internal readonly HashSet<Type> excludedTypes = new();
        internal readonly HashSet<(Type, string)> excludedFields = new();
        internal readonly Dictionary<Type, Func<object, string>> typesPrinting = new();
        internal readonly Dictionary<(Type, string), Func<object, string>> fieldsPrinting = new();
        internal readonly Dictionary<Type, CultureInfo> typesCultureInfo = new();
        internal readonly Dictionary<(Type, string), int> stringFieldsTrim = new();
        internal static readonly Type[] PrimitiveTypes = new[] {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(decimal),
            typeof(long), typeof(short), typeof(byte), typeof(Guid)
        };
        internal Dictionary<object, int> printedObjects = new Dictionary<object, int>(ReferenceEqualityComparer.Instance);
        internal int recursionLimit { get; private set; } = 10;

        public PrintingConfig<TOwner> WithRecursionLimit(int limit)
        {
            if (limit < 0)
                throw new ArgumentException("Лимит рекурсии не может быть отрицательным");
            recursionLimit = limit;
            return this;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            if (typeof(TPropType) == typeof(TOwner))
            {
                throw new InvalidOperationException("Нельзя настроить сериализацию для класса сериализации");
            }
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberExpression = (MemberExpression)memberSelector.Body;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberExpression.Member);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberExpression = (MemberExpression)memberSelector.Body;
            excludedFields.Add((memberExpression.Member.DeclaringType!, memberExpression.Member.Name));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            if (typeof(TPropType) == typeof(TOwner))
            {
                throw new InvalidOperationException("Нельзя исключить из сериализации сам класс сериализации");
            }

            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner? obj)
        {
            printedObjects = new Dictionary<object, int>(ReferenceEqualityComparer.Instance);
            return ObjectPrinter.PrintToString(this, obj);
        }

        internal void AddNewTypePrintingFunc(Type type, Func<object, string> printingFunc)
        {
            typesPrinting[type] = printingFunc;
        }

        internal void AddNewFieldPrintingFunc(Type declaringType, string fieldName, Func<object, string> printingFunc)
        {
            fieldsPrinting[(declaringType, fieldName)] = printingFunc;
        }

        internal void AddNewTypeCultureInfo(Type type, CultureInfo cultureInfo)
        {
            typesCultureInfo[type] = cultureInfo;
        }

        internal void AddNewFieldTrim(Type declaringType, string fieldName, int maxLength)
        {
            stringFieldsTrim[(declaringType, fieldName)] = maxLength;
        }
    }
}