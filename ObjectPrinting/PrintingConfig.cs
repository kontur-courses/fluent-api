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
        private readonly HashSet<string> excludedTypes = new HashSet<string>();

        private readonly HashSet<string> excludedProperties = new HashSet<string>();

        private readonly Dictionary<Type, Func<object, string>> specialPrintingFunctionsForTypes =
            new Dictionary<Type, Func<object, string>>();

        private readonly Dictionary<PropertyInfo, Func<object, string>> specialPrintingFunctionsForProperties =
            new Dictionary<PropertyInfo, Func<object, string>>();

        private readonly Dictionary<object, int> objectsLevels = new Dictionary<object, int>();

        private int collectionsPrintCount = 10;

        private readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, specialPrintingFunctionsForTypes);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(
                this, specialPrintingFunctionsForProperties, ExtractPropertyInfo(memberSelector));
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludedProperties.Add(
                ConstructPropertyFullName(typeof(TOwner), 
                    ExtractPropertyInfo(memberSelector)));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            var type = typeof(TPropType);
            excludedTypes.Add(type.FullName);
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private static PropertyInfo ExtractPropertyInfo<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body is MemberExpression memberExpression)
                return (PropertyInfo) memberExpression.Member;
            throw new ArgumentException("Member selector should be member expression");
        }

        private string PrintToString(object obj, int nestingLevel, bool newLineRequested = true)
        {
            if (obj == null)
                return $"null{(newLineRequested ? Environment.NewLine : "")}";
            objectsLevels[obj] = nestingLevel;
            if (TryToPrintType(obj, out var printed, nestingLevel, newLineRequested))
                return printed;
            return PrintProperties(obj, nestingLevel);
        }

        private bool TryToPrintType(object obj, out string result, int nestingLevel, bool newLineRequested = true)
        {
            var type = obj.GetType();
            result = null;
            if (excludedTypes.Contains(type.FullName))
                result = "";
            else if (specialPrintingFunctionsForTypes.TryGetValue(type, out var print))
                result = print(obj) + Environment.NewLine;
            else if (finalTypes.Contains(type))
                result = obj + (newLineRequested ? Environment.NewLine : "");
            else if (obj is IDictionary dictionary)
                result = PrintDictionary(dictionary, nestingLevel);
            else if (obj is IEnumerable enumerable)
                result = PrintEnumerable(enumerable, nestingLevel);
            return result != null;
        }

        private string PrintEnumerable(IEnumerable enumerable, int nestingLevel)
        {
            var objectsEnumerable = enumerable.Cast<object>();
            var printedObjects = objectsEnumerable
                .Select(n => PrintToString(n, nestingLevel + 1, false))
                .Take(collectionsPrintCount);
            return $"{string.Join("; ", printedObjects)}" +
                   $"{(printedObjects.Count() == collectionsPrintCount ? " ..." : "")}" +
                   $"{Environment.NewLine}";
        }

        private string PrintDictionary(IDictionary dictionary, int nestingLevel)
        {
            var sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            foreach (DictionaryEntry pair in dictionary)
            {
                var key = pair.Key;
                var value = pair.Value;
                var printedKey = TryToPrintType(key, out var result, 0, false)
                    ? result
                    : key.GetType().ToString();
                var printedValue = PrintToString(value, nestingLevel + 2, false);
                var indentation = new string('\t', nestingLevel + 1);
                var entry = $"{printedKey} = {printedValue}";
                sb.Append($"{indentation}{entry}{Environment.NewLine}");
            }
            sb.Append(Environment.NewLine);
            return sb.ToString();
        }

        private string PrintProperties(object obj, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                var name = propertyInfo.Name;
                var value = propertyInfo.GetValue(obj);
                string entry;
                if (excludedProperties.Contains(ConstructPropertyFullName(type, propertyInfo))
                    || (value != null && excludedTypes.Contains(value.GetType().FullName)))
                {
                    continue;
                }
                if (value != null && objectsLevels.ContainsKey(value) && objectsLevels[value] < nestingLevel)
                {
                    entry = $"{name} contains cyclic reference{Environment.NewLine}";
                }
                else if (specialPrintingFunctionsForProperties.TryGetValue(propertyInfo, out var print))
                {
                    entry = $"{print(value)}{Environment.NewLine}";
                }
                else
                {
                    entry = $"{name} = {PrintToString(value, nestingLevel + 1)}";
                }
                sb.Append($"{indentation}{entry}");
            }
            return sb.ToString();
        }

        private static string ConstructPropertyFullName(Type type, PropertyInfo propertyInfo)
        {
            return $"{type.FullName}.{propertyInfo.Name}";
        }
    }
}