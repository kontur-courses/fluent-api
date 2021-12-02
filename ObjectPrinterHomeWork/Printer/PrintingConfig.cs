using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;

namespace Printer
{
    public class PrintingConfig<TOwner>
    {
        private readonly Dictionary<Type, Func<object, string>> typeCustomSerializers = new();
        private readonly Dictionary<FieldInfo, Func<FieldInfo, string, string>> fieldCustomSerializers = new();
        private readonly Dictionary<PropertyInfo, Func<PropertyInfo, string, string>> propertyCustomSerializers = new();


        private readonly List<Type> excludedTypes = new();
        private readonly HashSet<MemberInfo> excludedMembers = new();

        private CultureInfo globalCulture = CultureInfo.CurrentCulture;
        private readonly Dictionary<Type, (CultureInfo formatProvider, string format)> formatProviders = new();

        private readonly HashSet<object> serializedObjects = new();

        private static readonly Type[] FinalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid), typeof(Enum)
        };

        public string PrintToString(TOwner obj, int recursionLimit = 50)
        {
            return PrintToString(obj, 0, recursionLimit);
        }

        public PrintingConfig<TOwner> Exclude<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> StringOf<T>(Func<T, string> serializer)
        {
            typeCustomSerializers[typeof(T)] = e => serializer((T)e);
            return this;
        }

        public PrintingConfig<TOwner> WithCultureFor<T>(CultureInfo formatProvider = null, string format = null)
            where T : IFormattable
        {
            formatProviders[typeof(T)] = (formatProvider, format);
            return this;
        }

        public PrintingConfig<TOwner> WithCulture(CultureInfo formatProvider)
        {
            globalCulture = formatProvider;
            return this;
        }

        public PrintingConfig<TOwner> ExcludeProperty(string propertyName)
        {
            var property = typeof(TOwner).GetProperty(propertyName);

            if (property != null)
            {
                excludedMembers.Add(property);
            }

            return this;
        }

        public PrintingConfig<TOwner> ExcludeField(string fieldName)
        {
            var field = typeof(TOwner).GetField(fieldName);

            if (field != null)
            {
                excludedMembers.Add(field);
            }

            return this;
        }

        public PrintingConfig<TOwner> StringOfField(string fieldName, Func<FieldInfo, string, string> serializer)
        {
            var field = typeof(TOwner).GetField(fieldName);

            if (field != null)
            {
                fieldCustomSerializers[field] = serializer;
            }

            return this;
        }

        public PrintingConfig<TOwner> StringOfProperty(string propertyName,
            Func<PropertyInfo, string, string> serializer)
        {
            var property = typeof(TOwner).GetProperty(propertyName);

            if (property != null)
            {
                propertyCustomSerializers[property] = serializer;
            }

            return this;
        }

        private string PrintToString(object obj, int nestingLevel, int recursionLimit = 50)
        {
            if (obj is null)
            {
                return "null";
            }

            if (nestingLevel >= recursionLimit)
            {
                return string.Empty;
            }

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            if (typeCustomSerializers.ContainsKey(type))
            {
                sb.Append(identation + typeCustomSerializers[type](obj));
                return sb.ToString();
            }

            if (FinalTypes.Contains(obj.GetType()))
            {
                return Serialize(obj);
            }

            if (!serializedObjects.Add(obj))
            {
                sb.Append($"({obj.GetHashCode()})");
                return sb.Replace("\r", "").Replace("\n", "").ToString();
            }


            foreach (var memberInfo
                in type.GetMembers().Where(member => member.MemberType is MemberTypes.Field or MemberTypes.Property))
            {
                if (IsNotExcludedMember(memberInfo, obj, nestingLevel, out var result))
                {
                    sb.Append(identation + result + Environment.NewLine);
                }
            }

            return sb.ToString();
        }

        private bool IsNotExcludedMember(MemberInfo memberInfo, object obj, int nestingLevel, out string result)
        {
            if (excludedMembers.Contains(memberInfo))
            {
                result = string.Empty;

                return false;
            }

            switch (memberInfo)
            {
                case PropertyInfo propertyInfo:
                    return IsNotExcludedMember(propertyInfo, obj, nestingLevel, out result);

                case FieldInfo fieldInfo:
                    return IsNotExcludedMember(fieldInfo, obj, nestingLevel, out result);

                default:
                    throw new Exception();
            }
        }

        private bool IsNotExcludedMember(FieldInfo fieldInfo, object obj, int nestingLevel, out string serializeField)
        {
            if (excludedTypes.Contains(fieldInfo.FieldType))
            {
                serializeField = string.Empty;
                return false;
            }

            var fieldType = fieldInfo.FieldType;

            var serializedTypeValue = typeCustomSerializers.ContainsKey(fieldType)
                ? typeCustomSerializers[fieldType](fieldInfo.GetValue(obj))
                : PrintToString(fieldInfo.GetValue(obj), nestingLevel + 1);

            serializeField = fieldCustomSerializers.ContainsKey(fieldInfo)
                ? fieldCustomSerializers[fieldInfo](fieldInfo, serializedTypeValue)
                : fieldInfo.Name + " = " + serializedTypeValue;

            return true;
        }

        private bool IsNotExcludedMember(PropertyInfo propertyInfo, object obj, int nestingLevel,
            out string serializedProperty)
        {
            if (excludedTypes.Contains(propertyInfo.PropertyType))
            {
                serializedProperty = string.Empty;
                return false;
            }

            var propertyType = propertyInfo.PropertyType;

            var serializedTypeValue = typeCustomSerializers.ContainsKey(propertyType)
                ? typeCustomSerializers[propertyType](propertyInfo.GetValue(obj))
                : PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);

            serializedProperty = propertyCustomSerializers.ContainsKey(propertyInfo)
                ? propertyCustomSerializers[propertyInfo](propertyInfo, serializedTypeValue)
                : propertyInfo.Name + " = " + serializedTypeValue;

            return true;
        }

        private string Serialize(object obj)
        {
            if (obj is not IFormattable formattable)
            {
                return obj.ToString();
            }

            var type = formattable.GetType();

            return formatProviders.ContainsKey(type)
                ? formattable.ToString(formatProviders[type].format, formatProviders[type].formatProvider)
                : formattable.ToString(null, globalCulture);
        }
    }
}