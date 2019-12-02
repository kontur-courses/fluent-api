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
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<MemberInfo> excludedMembers = new HashSet<MemberInfo>();

        private readonly Dictionary<Type, Func<object, string>> specialPrintingFunctionsForTypes =
            new Dictionary<Type, Func<object, string>>();

        private readonly Dictionary<MemberInfo, Func<object, string>> specialPrintingFunctionsForMembers =
            new Dictionary<MemberInfo, Func<object, string>>();

        private readonly Dictionary<object, int> objectsLevels = new Dictionary<object, int>();

        private int collectionsPrintCount = 10;

        private readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid), typeof(bool)
        };

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, specialPrintingFunctionsForTypes);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(
                this, specialPrintingFunctionsForMembers, ExtractMemberInfo(memberSelector));
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludedMembers.Add(ExtractMemberInfo(memberSelector));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            var type = typeof(TPropType);
            excludedTypes.Add(type);
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private static MemberInfo ExtractMemberInfo<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body is MemberExpression memberExpression)
                return memberExpression.Member;
            throw new ArgumentException("Member selector should be member expression");
        }

        private string PrintToString(object obj, int nestingLevel, bool newLineRequested = true)
        {
            if (obj == null)
                return $"null{(newLineRequested ? Environment.NewLine : "")}";
            objectsLevels[obj] = nestingLevel;
            if (TryToPrintType(obj, out var printed, nestingLevel, newLineRequested))
                return printed;
            return PrintFieldsAndProperties(obj, nestingLevel);
        }

        private bool TryToPrintType(object obj, out string result, int nestingLevel, bool newLineRequested = true)
        {
            var type = obj.GetType();
            result = null;
            if (excludedTypes.Contains(type))
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
                .Select((n, i) =>
                    i < collectionsPrintCount
                        ? PrintToString(n, nestingLevel + 1, false)
                        : "...")
                .Take(collectionsPrintCount + 1);
            return $"[{string.Join(", ", printedObjects)}]{Environment.NewLine}";
        }

        //private string PrintEnumerable(IEnumerable enumerable, int nestingLevel)
        //{
        //    var objectsEnumerable = enumerable.Cast<object>();
        //    var printedObjects = objectsEnumerable
        //        .Select(n => PrintToString(n, nestingLevel + 1, false))
        //        .Take(collectionsPrintCount);
        //    return $"{string.Join("; ", printedObjects)}" +
        //           $"{(printedObjects.Count() == collectionsPrintCount ? " ..." : "")}" +
        //           $"{Environment.NewLine}";
        //}

        private string PrintDictionary(IDictionary dictionary, int nestingLevel)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append(Environment.NewLine);
            var indentation = new string('\t', nestingLevel + 1);
            foreach (DictionaryEntry pair in dictionary)
            {
                var key = pair.Key;
                var value = pair.Value;
                var printedKey = TryToPrintType(key, out var result, 0, false)
                    ? result
                    : key.GetType().ToString();
                var printedValue = PrintToString(value, nestingLevel + 2, false);
                var entry = $"{printedKey} = {printedValue}";
                sb.Append($"{indentation}{entry}{Environment.NewLine}");
            }
            sb.Append($"{new string('\t', nestingLevel)}}}{Environment.NewLine}");
            return sb.ToString();
        }

        private string PrintFieldsAndProperties(object obj, int nestingLevel)
        {
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var memberInfo in GetFieldsAndProperties(type))
            {
                var name = memberInfo.Name;
                var value = GetMemberValue(memberInfo, obj);
                string entry;
                if (excludedMembers.Contains(memberInfo) 
                    || (value != null && excludedTypes.Contains(value.GetType())))
                    continue;
                if (ContainsCycleReference(value, nestingLevel))
                    entry = $"{name} contains cyclic reference{Environment.NewLine}";
                else if (specialPrintingFunctionsForMembers.TryGetValue(memberInfo, out var print))
                    entry = $"{print(value)}{Environment.NewLine}";
                else
                    entry = $"{name} = {PrintToString(value, nestingLevel + 1)}";
                sb.Append($"{indentation}{entry}");
            }
            return sb.ToString();
        }

        private bool ContainsCycleReference(object value, int nestingLevel)
        {
            return value != null && objectsLevels.TryGetValue(value, out var savedLevel) && savedLevel < nestingLevel;
        }

        private IEnumerable<MemberInfo> GetFieldsAndProperties(Type type)
        {
            foreach (var fieldInfo in type.GetFields())
                yield return fieldInfo;
            foreach (var propertyInfo in type.GetProperties())
                yield return propertyInfo;
        }

        private static object GetMemberValue(MemberInfo memberInfo, object obj)
        {
            switch (memberInfo)
            {
                case PropertyInfo propertyInfo:
                    return propertyInfo.GetValue(obj);
                case FieldInfo fieldInfo:
                    return fieldInfo.GetValue(obj);
                default:
                    throw new ArgumentException("Member info should be an info with value");
            }
        }
    }
}