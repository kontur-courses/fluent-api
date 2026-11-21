using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private static readonly HashSet<Type> FinalTypes =
        [
            typeof(int), typeof(double), typeof(float),
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
            AppendValue(obj, 0, [], sb);

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
                if (obj is IEnumerable collection and not string)
                    SerializeCollection(collection, nestingLevel, processedObjects, sb);
                else
                    SerializeObject(obj, nestingLevel, processedObjects, sb);
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

            foreach (var propertyInfo in type.GetProperties())
            {
                if (propertyInfo.GetIndexParameters().Length > 0 ||
                    excludedTypes.Contains(propertyInfo.PropertyType) ||
                    excludedProperties.Contains(propertyInfo))
                    continue;

                var value = propertyInfo.GetValue(obj);
                sb.Append('\t', nestingLevel + 1)
                    .Append(propertyInfo.Name)
                    .Append(" = ");

                if (propertySerializers.TryGetValue(propertyInfo, out var serializer))
                    sb.AppendLine(serializer(value!));
                else
                    AppendValue(value, nestingLevel + 1, processedObjects, sb);
            }
        }

        private void SerializeCollection(
            IEnumerable collection,
            int nestingLevel,
            HashSet<object> processedObjects,
            StringBuilder sb)
        {
            sb.AppendLine(collection.GetType().Name);

            foreach (var item in collection)
            {
                sb.Append('\t', nestingLevel + 1);
                AppendValue(item, nestingLevel + 1, processedObjects, sb);
            }
        }
    }
}
