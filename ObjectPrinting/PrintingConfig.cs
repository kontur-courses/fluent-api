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
        private class Comparator<T> : EqualityComparer<T>
        {
            public override bool Equals(T x, T y) => x != null && ReferenceEquals(x, y);
            public override int GetHashCode(T obj) => 0;
        }

        private readonly HashSet<MemberInfo> excludedMembers = new HashSet<MemberInfo>();
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<object> currentPassed = new HashSet<object>(new Comparator<object>());

        public Dictionary<Type, Delegate> TypeSerializers { get; } = new Dictionary<Type, Delegate>();
        public Dictionary<MemberInfo, Delegate> PropertySerializers { get; } = new Dictionary<MemberInfo, Delegate>();
        public Dictionary<Type, CultureInfo> TypesCultures { get; } = new Dictionary<Type, CultureInfo>();
        public Dictionary<MemberInfo, int> StringPropertyLengths { get; } = new Dictionary<MemberInfo, int>();

        public PropertyPrintingConfig<TOwner, TProperty> ForType<TProperty>()
        {
            return new PropertyPrintingConfig<TOwner, TProperty>(this);
        }

        public PropertyPrintingConfig<TOwner, TProperty> ForProperty<TProperty>(
            Expression<Func<TOwner, TProperty>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TProperty>(this, memberSelector);
        }

        public PrintingConfig<TOwner> Excluding<TProperty>()
        {
            excludedTypes.Add(typeof(TProperty));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TProperty>(Expression<Func<TOwner, TProperty>> memberSelector)
        {
            if (memberSelector.Body is MemberExpression memberExpression)
                excludedMembers.Add(memberExpression.Member);
            else
                throw new ArgumentException("Expression contains not MemberExpression");
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            var result = PrintToString(obj, 0);
            currentPassed.Clear();
            return result;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj is null)
                return "null";

            var hasLoop = currentPassed.Contains(obj);
            if (hasLoop)
                return obj.GetType().FullName;

            var isFinalType = IsFinalType(obj.GetType());

            if (!isFinalType)
                currentPassed.Add(obj);

            var indentation = GetIndentation(nestingLevel);

            if (TypeSerializers.TryGetValue(obj.GetType(), out var serializer))
            {
                currentPassed.Remove(obj);
                return indentation + (string)serializer.DynamicInvoke(obj);
            }

            if (TypesCultures.TryGetValue(obj.GetType(), out var cultureInfo) && obj is IFormattable formattable)
            {
                currentPassed.Remove(obj);
                return indentation + formattable.ToString(null, cultureInfo);
            }

            if (isFinalType)
            {
                currentPassed.Remove(obj);
                return obj.ToString();
            }

            if (TryPrintIDictionary(obj as IDictionary, nestingLevel, out var serialized)
                || TryPrintIEnumerable(obj as IEnumerable, nestingLevel, out serialized))
            {
                currentPassed.Remove(obj);
                return serialized;
            }

            var result = /*Environment.NewLine + */PrintPublicMembers(obj, nestingLevel + 1);
            currentPassed.Remove(obj);

            return result;
        }

        private static bool IsFinalType(Type type)
        {
            return (type.IsValueType
                    || type.IsPrimitive
                    || type == typeof(string))
                   && !typeof(IList).IsAssignableFrom(type)
                   && !typeof(IDictionary).IsAssignableFrom(type);
        }

        private static string GetIndentation(int nestingLevel)
        {
            return new string('\t', nestingLevel);
        }

        private static object GetValue(object obj, MemberInfo memberInfo)
        {
            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
                return propertyInfo.GetValue(obj);

            var fieldInfo = memberInfo as FieldInfo;
            return fieldInfo != null ? fieldInfo.GetValue(obj) : null;
        }

        private bool IsPropertyExcluded(PropertyInfo info)
        {
            return excludedTypes.Contains(info.PropertyType) || excludedMembers.Contains(info);
        }

        private bool IsFieldExcluded(FieldInfo fieldInfo)
        {
            return fieldInfo.IsStatic
                   || !fieldInfo.IsPublic
                   || excludedTypes.Contains(fieldInfo.FieldType)
                   || excludedMembers.Contains(fieldInfo);
        }

        private bool TryPrintIEnumerable(IEnumerable obj, int nestingLevel, out string result)
        {
            result = string.Empty;
            if (obj is null)
                return false;

            var sb = new StringBuilder();

            var hasElements = false;
            foreach (var i in obj)
            {
                sb.Append(GetIndentation(nestingLevel + 2));
                sb.Append(PrintToString(i, nestingLevel + 1));
                sb.AppendLine(",");

                hasElements = true;
            }

            var removeCount = Environment.NewLine.Length;
            removeCount += hasElements ? 1 : 0;
            if (sb.Length >= removeCount)
                sb.Remove(sb.Length - removeCount, removeCount);

            result = PrintInBrackets("[", "]", sb.ToString(), nestingLevel + 1);
            return true;
        }

        private bool TryPrintIDictionary(IDictionary obj, int nestingLevel, out string result)
        {
            result = string.Empty;
            if (obj is null)
                return false;

            var sb = new StringBuilder();
            var hasElements = false;

            foreach (var i in obj.Keys)
            {
                sb.Append(GetIndentation(nestingLevel + 2));
                sb.AppendLine($"[{i}] = {PrintToString(obj[i], nestingLevel + 2)},");
                hasElements = true;
            }

            var removeCount = Environment.NewLine.Length;
            removeCount += hasElements ? 1 : 0;
            if (sb.Length >= removeCount)
                sb.Remove(sb.Length - removeCount, removeCount);

            result = PrintInBrackets("{", "}", sb.ToString(), nestingLevel + 1);
            return true;
        }

        private static string PrintInBrackets(string openBracket, string endBracket, string text, int nestingLevel)
        {
            var sb = new StringBuilder(text);
            var indention = GetIndentation(nestingLevel);
            sb.Insert(0, Environment.NewLine + indention + openBracket + Environment.NewLine);
            sb.Append(Environment.NewLine + indention + endBracket);

            return sb.ToString();
        }

        private string PrintMember(object value, MemberInfo memberInfo, int nestingLevel)
        {
            string valueString;

            if (PropertySerializers.TryGetValue(memberInfo, out var serializer))
                valueString = (string)serializer.DynamicInvoke(value);
            else
                valueString = PrintToString(value, nestingLevel + 1);

            if (StringPropertyLengths.TryGetValue(memberInfo, out var length))
                valueString = valueString?[..length];

            return Environment.NewLine + GetIndentation(nestingLevel + 1) + memberInfo.Name + " = " + valueString;
        }

        private string PrintPublicMembers(object obj, int nestingLevel)
        {
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.Append(type.Name);
            sb.Append(":");
            foreach (var info in GetPublicPropertiesAndFieldsInfos(type))
                sb.Append(PrintMember(GetValue(obj, info), info, nestingLevel));

            return sb.ToString();
        }

        private IEnumerable<MemberInfo> GetPublicPropertiesAndFieldsInfos(Type type)
        {
            return type.GetProperties().Where(info => !IsPropertyExcluded(info))
                .Concat<MemberInfo>(type.GetFields().Where(info => !IsFieldExcluded(info)));
        }
    }
}