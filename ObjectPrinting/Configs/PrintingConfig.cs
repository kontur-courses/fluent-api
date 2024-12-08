using ObjectPrinting.Utilits.Strings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Configs
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> _excludedTypes
            = new HashSet<Type>();
        private readonly HashSet<MemberInfo> _excludedProperties
            = new HashSet<MemberInfo>();
        private readonly Dictionary<Type, Delegate> _specialTypeProcessing
            = new Dictionary<Type, Delegate>();
        private readonly Dictionary<MemberInfo, Delegate> _specialProperyProcessing
            = new Dictionary<MemberInfo, Delegate>();
        private readonly Dictionary<MemberInfo, int> _trimmedProperties
            = new Dictionary<MemberInfo, int>();
        private readonly HashSet<object> _processedObjects
            = new HashSet<object>();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
            => new(this);

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberExpression = (MemberExpression)memberSelector.Body;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberExpression.Member);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberExpression = (MemberExpression)memberSelector.Body;
            _excludedProperties.Add(memberExpression.Member);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            _excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            ArgumentNullException.ThrowIfNull(obj);
            return PrintToString(obj, 0).TrimEnd(Environment.NewLine);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
            {
                return "null";
            }

            var currentType = obj.GetType();

            if (_specialTypeProcessing.TryGetValue(currentType, out var correctDelegate))
            {
                return correctDelegate.DynamicInvoke(obj)!.ToString()!;
            }

            if (IsFinalType(currentType))
            {
                return obj.ToString()!;
            }

            if (!_processedObjects.Add(obj))
            {
                throw new OverflowException("Cycle detected");
            }

            if (obj as ICollection != null)
            {
                return ProcessCollection(obj, nestingLevel);
            }

            var result = new StringBuilder();
            var identation = StringUtilits.GetIdentation(nestingLevel + 1);

            result.AppendLine(currentType.Name);
            foreach (var propertyInfo in currentType.GetProperties())
            {
                if (IsSkipping(propertyInfo))
                {
                    continue;
                }

                result.Append($"{identation}{propertyInfo.Name} = ");
                if (IsTrimming(propertyInfo))
                {
                    result.AppendLine(TrimValue(obj, propertyInfo));
                    continue;
                }

                if (IsSpecialProcessing(propertyInfo))
                {
                    result.AppendLine(GetSpeciallyProcessedValue(obj, propertyInfo));
                    continue;
                }
                result.AppendLine(PrintToString(propertyInfo.GetValue(obj)!, nestingLevel + 1));
            }
            return result.ToString();
        }

        private static bool IsFinalType(Type type)
            => type.IsPrimitive || type == typeof(string) || type == typeof(Guid);

        private bool IsSkipping(PropertyInfo propertyInfo)
            => _excludedTypes.Contains(propertyInfo.PropertyType)
                    || _excludedProperties.Contains(propertyInfo);

        private bool IsTrimming(PropertyInfo propertyInfo)
            => _trimmedProperties.ContainsKey(propertyInfo);

        private bool IsSpecialProcessing(PropertyInfo propertyInfo)
            => _specialProperyProcessing.ContainsKey(propertyInfo);

        private string ProcessCollection(object obj, int nestingLevel)
        {
            var currentLevelNesting = StringUtilits.GetIdentation(nestingLevel);
            nestingLevel += 1;

            var result = new StringBuilder();
            result.Append($"{Environment.NewLine}{currentLevelNesting}{{{Environment.NewLine}");

            if (obj as IDictionary != null)
            {
                var dictionary = obj as IDictionary;
                ProcessDictionary(dictionary!, nestingLevel, result);
            }
            else if (obj as IEnumerable != null)
            {
                var enumerable = obj as IEnumerable;
                ProcessEnumerable(enumerable!, nestingLevel, result);
            }

            result.Append($"{currentLevelNesting}}}");
            return result.ToString();
        }

        private void ProcessDictionary(
            IDictionary dictionary,
            int nestingLevel,
            StringBuilder stringBuilder)
        {
            foreach (var key in dictionary.Keys)
            {
                var value = dictionary[key];
                stringBuilder.Append(
                    $"{StringUtilits.GetIdentation(nestingLevel)}" +
                    $"{PrintToString(key, nestingLevel)} = {PrintToString(value!, nestingLevel)}");

                var innerItemType = value!.GetType();
                if (IsFinalType(innerItemType))
                {
                    stringBuilder.AppendLine();
                }
            }
        }

        private void ProcessEnumerable(
            IEnumerable enumerable,
            int nestingLevel,
            StringBuilder stringBuilder)
        {
            foreach (var value in enumerable)
            {
                stringBuilder.Append(
                    $"{StringUtilits.GetIdentation(nestingLevel)}" +
                    $"{PrintToString(value, nestingLevel)}");

                var innerItemType = value!.GetType();
                if (IsFinalType(innerItemType))
                {
                    stringBuilder.AppendLine();
                }
            }
        }

        private string TrimValue(object obj, PropertyInfo propertyInfo)
        {
            ArgumentNullException.ThrowIfNull(obj);
            var value = propertyInfo.GetValue(obj)!;
            var length = _trimmedProperties[propertyInfo];
            return value.ToString()!.Substring(0, length);
        }

        private string GetSpeciallyProcessedValue(object obj, PropertyInfo propertyInfo)
        {
            ArgumentNullException.ThrowIfNull(obj);
            var value = propertyInfo.GetValue(obj)!;
            var transformation = _specialProperyProcessing[propertyInfo]!;
            return transformation.DynamicInvoke(value)!.ToString()!;
        }

        internal void AddSpecialTypeProcessing<TPropType>(Func<TPropType, string> print)
        {
            _specialTypeProcessing[typeof(TPropType)] = print;
        }

        internal void AddSpecialProperyProcessing<TPropType>(
            Func<TPropType, string> print,
            MemberInfo memberInfo)
        {
            _specialProperyProcessing[memberInfo] = print;
        }

        internal void AddTrimmedPropertiesProcessing(
            int length,
            MemberInfo memberInfo)
        {
            _trimmedProperties[memberInfo] = length;
        }
    }
}