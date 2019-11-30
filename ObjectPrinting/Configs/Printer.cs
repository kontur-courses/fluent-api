using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Configs
{
    internal class Printer<TOwner>
    {
        private readonly HashSet<Type> excludedTypes;
        private readonly HashSet<MemberInfo> excludedMembers;
        private readonly Dictionary<Type, IPropertySerializingConfig<TOwner>> typesConfigs;
        private readonly Dictionary<MemberInfo, IPropertySerializingConfig<TOwner>> membersConfig;

        private readonly HashSet<object> printed;
        private readonly StringBuilder output;

        private static readonly List<Type> finalTypes = new List<Type>
        {
            typeof(string),
            typeof(DateTime),
            typeof(TimeSpan)
        };

        public Printer(HashSet<Type> excludedTypes, HashSet<MemberInfo> excludedMembers,
            Dictionary<Type, IPropertySerializingConfig<TOwner>> typesConfigs,
            Dictionary<MemberInfo, IPropertySerializingConfig<TOwner>> membersConfig)
        {
            this.excludedTypes = excludedTypes;
            this.excludedMembers = excludedMembers;
            this.typesConfigs = typesConfigs;
            this.membersConfig = membersConfig;

            printed = new HashSet<object>();
            output = new StringBuilder();
        }

        public string PrintToString(TOwner objectToPrint)
        {
            Print(objectToPrint);

            return output.ToString();
        }

        private void Print(object objectToPrint)
        {
            var type = objectToPrint.GetType();

            if (typesConfigs.TryGetValue(type, out var config))
            {
                var serialize = config.Serialize;

                output.Append(serialize(objectToPrint));
            }
            else if (finalTypes.Contains(type) || type.IsPrimitive)
            {
                output.Append(objectToPrint);
            }
            else if (objectToPrint is IEnumerable enumerable)
            {
                printed.Add(objectToPrint);
                PrintCollectionValue(enumerable);
            }
            else
            {
                printed.Add(objectToPrint);
                PrintRegularObjectValue(objectToPrint, -1);
            }
        }

        private void Print(PropertyInfo propertyInfo, object value, int nestingLevel)
        {
            var indent = new string('\t', nestingLevel);

            if (value == null)
            {
                output.Append(indent).Append(propertyInfo.Name).Append(" = ").Append("null");
                return;
            }

            var isReferenceType = !propertyInfo.PropertyType.IsValueType;

            if (isReferenceType && printed.Contains(value))
            {
                output.Append(indent).Append(propertyInfo.Name).Append(" = ").Append("[Cyclic reference detected]");
                return;
            }

            if (isReferenceType)
                printed.Add(value);

            PrintProperty(propertyInfo, value, nestingLevel);
        }

        private void PrintProperty(PropertyInfo propertyInfo, object value, int nestingLevel)
        {
            if (TryGetSerialize(propertyInfo, out var serialize)
                || finalTypes.Contains(propertyInfo.PropertyType)
                || propertyInfo.PropertyType.IsPrimitive)
                PrintElementaryProperty(propertyInfo, value, serialize ?? (x => x.ToString()), nestingLevel);
            else
                PrintComplexObject(propertyInfo, value, nestingLevel);
        }

        private void PrintElementaryProperty(PropertyInfo propertyInfo, object value, Func<object, string> serialize,
            int nestingLevel)
        {
            var indent = new string('\t', nestingLevel);
            var name = propertyInfo.Name;

            output.Append(indent).Append(name).Append(" = ").Append(serialize(value));
        }

        private void PrintComplexObject(PropertyInfo propertyInfo, object value, int nestingLevel)
        {
            if (value is IEnumerable enumerable)
                PrintCollection(propertyInfo, enumerable, nestingLevel);
            else
                PrintRegularObject(propertyInfo, value, nestingLevel);
        }

        private void PrintRegularObject(PropertyInfo propertyInfo, object value, int nestingLevel)
        {
            var indent = new string('\t', nestingLevel);
            var name = propertyInfo.Name;

            if (propertyInfo.PropertyType.GetProperties().Count(p => !IsExcluded(p)) == 0)
            {
                output.Append(indent).Append(name).Append(" = ").Append(value);
                return;
            }

            output.Append(indent).Append(name).Append(Environment.NewLine);

            PrintRegularObjectValue(value, nestingLevel);
        }

        private void PrintCollectionValue(IEnumerable enumerable)
        {
            if (ElementsIsExcluded(enumerable))
            {
                output.Append("[]");
                return;
            }


            output.Append("[");
            var i = 0;

            foreach (var obj in enumerable)
            {
                if (i != 0)
                {
                    output.Append(",").Append(Environment.NewLine);
                }
                
                var isReferenceType = !obj.GetType().IsValueType;
                if (isReferenceType && printed.Contains(obj))
                {
                    output.Append("[Cyclic reference detected]");
                    continue;
                }

                if (isReferenceType)
                    printed.Add(obj);

                Print(obj);
                i++;
            }

            output.Append("]").Append(Environment.NewLine);
        }

        private bool ElementsIsExcluded(IEnumerable enumerable)
        {
            var enumerator = enumerable.GetEnumerator();
            enumerator.MoveNext();

            return enumerator.Current != null && excludedTypes.Contains(enumerator.Current.GetType());
        }


        private void PrintRegularObjectValue(object value, int nestingLevel)
        {
            var properties = value.GetType().GetProperties().Where(p => !IsExcluded(p)).ToList();

            for (var i = 0; i < properties.Count; i++)
            {
                var property = properties[i];

                Print(property, property.GetValue(value), nestingLevel + 1);

                if (i != properties.Count - 1)
                    output.Append(Environment.NewLine);
            }
        }

        private void PrintCollection(PropertyInfo propertyInfo, IEnumerable value, int nestingLevel)
        {
            var indent = new string('\t', nestingLevel);

            output.Append(indent).Append(propertyInfo.Name).Append(" = ");

            PrintCollectionValue(value);
        }

        private bool IsExcluded(PropertyInfo propertyInfo)
        {
            return excludedTypes.Contains(propertyInfo.PropertyType) ||
                   excludedMembers.Contains(propertyInfo);
        }

        private bool TryGetSerialize(PropertyInfo propertyInfo, out Func<object, string> serialize)
        {
            if (membersConfig.TryGetValue(propertyInfo, out var config))
            {
                serialize = config.Serialize;
                return true;
            }

            if (typesConfigs.TryGetValue(propertyInfo.PropertyType, out config))
            {
                serialize = config.Serialize;
                return true;
            }

            serialize = null;
            return false;
        }
    }
}