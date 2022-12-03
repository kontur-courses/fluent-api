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
        private readonly HashSet<MemberInfo> excludedMembers = new HashSet<MemberInfo>();
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();

        private readonly HashSet<object> printed = new HashSet<object>();
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
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            var result = PrintToString(obj, 0);
            printed.Clear();
            return result;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj is null)
                return "null";

            if (printed.Contains(obj))
                return obj.GetType().FullName;

            var isFinalType = IsFinalType(obj.GetType());

            if (!isFinalType)
                printed.Add(obj);

            if (TypeSerializers.TryGetValue(obj.GetType(), out var serializer))
                return (string)serializer.DynamicInvoke(obj);
            if (TypesCultures.TryGetValue(obj.GetType(), out var cultureInfo) && obj is IFormattable formattable)
                return formattable.ToString(null, cultureInfo);
            if (obj.GetType() != typeof(string) &&
                TryPrintIDictionary(obj as IDictionary, nestingLevel, out var serializedDictionary))
                return serializedDictionary;
            if (obj.GetType() != typeof(string) &&
                TryPrintIEnumerable(obj as IEnumerable, nestingLevel, out var serializedEnumerable))
                return serializedEnumerable;

            if (isFinalType)
            {
                printed.Remove(obj);
                return obj.ToString();
            }

            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.Append(type.Name);
            var propertiesAndFieldInfos = type.GetProperties().Where(info => !IsPropertyExcluded(info))
                .Concat<MemberInfo>(type.GetFields().Where(info => !IsFieldExcluded(info)));
            foreach (var info in propertiesAndFieldInfos)
                sb.Append(PrintMember(GetValue(obj, info), info, nestingLevel));

            printed.Remove(obj);

            return sb.ToString();
        }

        private static bool IsFinalType(Type type)
        {
            return type.GetProperties().Length + type.GetFields().Length == 0
                   || typeof(IFormattable).IsAssignableFrom(type)
                   || type.IsDefined(typeof(SerializableAttribute));
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
            var hasBasicTypes = false;
            foreach (var i in obj)
            {
                sb.Append(PrintToString(i, nestingLevel + 1) + ", ");
                hasElements = true;
                hasBasicTypes = IsFinalType(i.GetType());
            }

            var removeCount = hasElements ? 2 : 1;
            if (sb.Length >= removeCount)
                sb.Remove(sb.Length - removeCount, removeCount);
            var indention = GetIndentation(nestingLevel);
            if (hasBasicTypes)
            {
                sb.Insert(0, "[ ");
                sb.Append(" ]");
            }
            else
            {
                sb.Insert(0, "[" + Environment.NewLine + indention);
                sb.Append(Environment.NewLine + indention + "]");
            }

            result = sb.ToString();
            return true;
        }

        private bool TryPrintIDictionary(IDictionary obj, int nestingLevel, out string result)
        {
            result = string.Empty;
            if (obj is null)
                return false;

            var indention = GetIndentation(nestingLevel);
            var sb = new StringBuilder();
            sb.Append("{" + Environment.NewLine + indention);
            var hasElements = false;

            foreach (var i in obj.Keys)
            {
                sb.Append(GetIndentation(nestingLevel + 1) + $"[{i}] = {PrintToString(obj[i], nestingLevel + 1)}," +
                          Environment.NewLine);
                hasElements = true;
            }

            var removeCount = hasElements ? 2 : 1;
            sb.Remove(sb.Length - removeCount, removeCount);
            sb.Append(Environment.NewLine + indention + "}");
            result = sb.ToString();
            return true;
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
    }
}