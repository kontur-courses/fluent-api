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
        private static readonly HashSet<Type> FinalTypes =
        [
            typeof(bool), typeof(int), typeof(double), typeof(float),
            typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        ];

        private readonly HashSet<Type> excludedTypes = [];
        private readonly HashSet<MemberInfo> excludedProperties = [];

        private readonly Dictionary<Type, Func<object, string>> typeSerializers = new();
        private readonly Dictionary<MemberInfo, Func<object, string>> propertySerializers = new();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberInfo = ((MemberExpression)memberSelector.Body).Member;

            return new PropertyPrintingConfig<TOwner, TPropType>(this, memberInfo);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberInfo = ((MemberExpression)memberSelector.Body).Member;
            excludedProperties.Add(memberInfo);

            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));

            return this;
        }

        internal void SetSerializer<TPropType>(Func<TPropType, string> serializer, MemberInfo? memberInfo = null)
        {
            if (memberInfo != null)
                propertySerializers[memberInfo] = value => serializer((TPropType)value);
            else
                typeSerializers[typeof(TPropType)] = value => serializer((TPropType)value);
        }

        public string PrintToString(TOwner obj)
        {
            var sb = new StringBuilder();
            var processedObjects = new HashSet<object>(ReferenceEqualityComparer.Instance);
            AppendValue(obj, 0, processedObjects, sb);

            return sb.ToString();
        }

        private void AppendValue(
            object? obj,
            int nestingLevel,
            HashSet<object> processedObjects,
            StringBuilder sb)
        {
            if (obj == null)
            {
                sb.AppendLine("null");
                return;
            }

            var type = obj.GetType();

            if (typeSerializers.TryGetValue(type, out var serializer))
            {
                sb.AppendLine(serializer(obj));
                return;
            }

            if (FinalTypes.Contains(type))
            {
                sb.Append(obj).AppendLine();
                return;
            }

            if (!type.IsValueType && !processedObjects.Add(obj))
            {
                sb.AppendLine("[Circular reference]");
                return;
            }

            try
            {
                switch (obj)
                {
                    case IDictionary dictionary:
                        SerializeDictionary(dictionary, nestingLevel, processedObjects, sb);
                        break;
                    case ICollection collection:
                        SerializeCollection(collection, nestingLevel, processedObjects, sb);
                        break;
                    default:
                        SerializeObject(obj, nestingLevel, processedObjects, sb);
                        break;
                }
            }
            finally
            {
                if (!type.IsValueType)
                    processedObjects.Remove(obj);
            }
        }

        private void SerializeObject(
            object obj,
            int nestingLevel,
            HashSet<object> processedObjects,
            StringBuilder sb)
        {
            var type = obj.GetType();

            sb.AppendLine(type.Name);

            var properties = type.GetProperties()
                .Where(property => property.GetIndexParameters().Length == 0);

            var fields = type
                .GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var member in properties.Concat<MemberInfo>(fields))
            {
                var memberType = member is PropertyInfo property
                    ? property.PropertyType
                    : ((FieldInfo)member).FieldType;

                if (excludedTypes.Contains(memberType) || excludedProperties.Contains(member))
                    continue;

                var value = member is PropertyInfo propertyInfo
                    ? propertyInfo.GetValue(obj)
                    : ((FieldInfo)member).GetValue(obj);

                sb.Append('\t', nestingLevel + 1)
                    .Append(member.Name)
                    .Append(" = ");

                if (propertySerializers.TryGetValue(member, out var serializer))
                    sb.AppendLine(serializer(value!));
                else
                    AppendValue(value, nestingLevel + 1, processedObjects, sb);
            }
        }

        private void SerializeDictionary(
            IDictionary dictionary,
            int nestingLevel,
            HashSet<object> processedObjects,
            StringBuilder sb)
        {
            sb.AppendLine("{");

            foreach (DictionaryEntry entry in dictionary)
            {
                sb.Append('\t', nestingLevel + 1);
                AppendValue(entry.Key, nestingLevel + 1, processedObjects, sb);
                sb.Length -= Environment.NewLine.Length;

                sb.Append(": ");
                AppendValue(entry.Value, nestingLevel + 1, processedObjects, sb);
            }

            sb.Append('\t', nestingLevel).AppendLine("}");
        }

        private void SerializeCollection(
            ICollection collection,
            int nestingLevel,
            HashSet<object> processedObjects,
            StringBuilder sb)
        {
            sb.AppendLine("[");

            foreach (var item in collection)
            {
                sb.Append('\t', nestingLevel + 1);
                AppendValue(item, nestingLevel + 1, processedObjects, sb);
            }

            sb.Append('\t', nestingLevel).AppendLine("]");
        }
    }
}
