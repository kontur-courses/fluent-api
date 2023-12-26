using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private static readonly HashSet<Type> _finalTypes = new()
        {
            typeof(int), typeof(uint), typeof(double), typeof(float), typeof(decimal),
            typeof(long), typeof(ulong), typeof(short), typeof(ushort),
            typeof(string), typeof(bool),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        };

        private readonly HashSet<MemberInfo> excludedProperties = new();
        private readonly HashSet<Type> excludedTypes = new();
        protected readonly Dictionary<MemberInfo, int> propertiesMaxLength = new();
        protected readonly Dictionary<MemberInfo, Func<object, string>> propertiesSerialization = new();
        protected readonly Dictionary<Type, CultureInfo> typesCulture = new();
        protected readonly Dictionary<Type, Func<object, string>> typesSerialization = new();
        private Func<dynamic> recursivePropertiesSerialization;

        public PrintingConfig()
        {
        }

        protected PrintingConfig(PrintingConfig<TOwner> config)
        {
            typesCulture = config.typesCulture;
            propertiesMaxLength = config.propertiesMaxLength;
            propertiesSerialization = config.propertiesSerialization;
            typesSerialization = config.typesSerialization;
        }

        public string PrintToString(TOwner obj)
        {
            return SerializeObject(obj, ImmutableList<object>.Empty);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));

            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> property)
        {
            var memberBody = (MemberExpression)property.Body;
            var propertyInfo = memberBody.Member;

            excludedProperties.Add(propertyInfo);

            return this;
        }

        public TypePrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> printProperty)
        {
            var memberBody = (MemberExpression)printProperty.Body;
            var propertyInfo = memberBody.Member;

            return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyInfo);
        }

        public PrintingConfig<TOwner> PrintRecursion(Exception exception)
        {
            recursivePropertiesSerialization = () => throw exception;

            return this;
        }

        public PrintingConfig<TOwner> PrintRecursion(Func<string> printProperty)
        {
            recursivePropertiesSerialization = printProperty;

            return this;
        }

        private string SerializeObject(object obj, IImmutableList<object> previousObjects)
        {
            if (obj == null)
                return "null";

            var type = obj.GetType();
            if (previousObjects.Any(prev => prev == obj))
            {
                if (recursivePropertiesSerialization != null)
                    return recursivePropertiesSerialization();
                return "Recursive property";
            }


            if (obj is IEnumerable && obj is not string)
                return SerializeEnumerable(obj, previousObjects);

            if (_finalTypes.Contains(type))
                return SerializeFinalType(obj);

            var identation = new string('\t', previousObjects.Count + 1);
            previousObjects = previousObjects.Add(obj);
            var objectSerialization = new StringBuilder();
            objectSerialization.AppendLine(type.Name);
            var members = Array.Empty<MemberInfo>().Concat(type.GetProperties()).Concat(type.GetFields());
            foreach (var memberInfo in members)
            {
                var memberType = memberInfo.MemberType == MemberTypes.Property
                    ? (memberInfo as PropertyInfo).PropertyType
                    : (memberInfo as FieldInfo).FieldType;
                if (excludedProperties.Contains(memberInfo)
                    || excludedTypes.Contains(memberType))
                    continue;

                objectSerialization.Append(identation);
                objectSerialization.Append(memberInfo.Name);
                objectSerialization.Append(" = ");
                objectSerialization.Append(SerializeMember(memberInfo, obj, previousObjects));
                objectSerialization.Append(Environment.NewLine);
            }

            return objectSerialization.ToString();
        }

        private string SerializeFinalType(object obj)
        {
            var type = obj.GetType();
            if (typesSerialization.TryGetValue(type, out var serialization))
                return serialization.Invoke(obj);

            var result = obj.ToString();
            if (typesCulture.ContainsKey(obj.GetType()))
                result = ((IFormattable)obj).ToString(null, typesCulture[type]);

            return result;
        }

        private string SerializeMember(MemberInfo memberInfo, object obj, IImmutableList<object> previousObjects)
        {
            var value = memberInfo.MemberType == MemberTypes.Property
                ? (memberInfo as PropertyInfo).GetValue(obj)
                : (memberInfo as FieldInfo).GetValue(obj);
            var serializedProperty = propertiesSerialization.TryGetValue(memberInfo, out var serialization)
                ? serialization.Invoke(value)
                : SerializeObject(value, previousObjects).TrimEnd();

            var propertyMaxLength = propertiesMaxLength.TryGetValue(memberInfo, out var length)
                ? length
                : serializedProperty.Length;

            serializedProperty = serializedProperty[..propertyMaxLength];

            return serializedProperty;
        }

        private string SerializeEnumerable(object obj, IImmutableList<object> previousObjects)
        {
            var enumerable = (IEnumerable)obj;
            var serializedEnumerable = new StringBuilder();
            var nestingLevel = previousObjects.Count == 0 ? 0 : previousObjects.Count + 1;
            var identation = nestingLevel == 0 ? "" : Environment.NewLine + new string('\t', nestingLevel);

            foreach (var item in enumerable)
            {
                serializedEnumerable.Append(identation);
                serializedEnumerable.Append(SerializeObject(item, previousObjects));
            }

            return serializedEnumerable.ToString();
        }
    }
}