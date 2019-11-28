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

        private static readonly List<Type> FinalTypes = new List<Type>
        {
            typeof(char),
            typeof(bool),
            typeof(ushort),
            typeof(uint),
            typeof(ulong),
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(double),
            typeof(float),
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
                var serializer = config.Serializer;

                output.Append(serializer(objectToPrint));
            }
            else if (FinalTypes.Contains(type))
            {
                output.Append(objectToPrint);
            }
            else if (objectToPrint is IEnumerable enumerable)
            {
                printed.Add(objectToPrint);
                PrintCollectionValue(enumerable, -1);
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
            Func<object, string> serializer = null;

            if (TryGetSerializer(propertyInfo, out serializer) || FinalTypes.Contains(propertyInfo.PropertyType))
                PrintElementaryProperty(propertyInfo, value, serializer ?? (x => x.ToString()), nestingLevel);
            else
                PrintComplexObject(propertyInfo, value, nestingLevel);
        }

        private void PrintElementaryProperty(PropertyInfo propertyInfo, object value, Func<object, string> serializer,
            int nestingLevel)
        {
            var indent = new string('\t', nestingLevel);
            var name = propertyInfo.Name;

            output.Append(indent).Append(name).Append(" = ").Append(serializer(value));
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


        private void PrintCollectionValue(IEnumerable enumerable, int nestingLevel)
        {
            output.Append("[");

            var i = 0;

            foreach (var obj in enumerable)
            {
                if (i != 0)
                {
                    output.Append(",").Append(Environment.NewLine);
                }

                Print(obj);
                i++;
            }

            output.Append("]").Append(Environment.NewLine);
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

            PrintCollectionValue(value, nestingLevel);
        }

        private bool IsExcluded(PropertyInfo propertyInfo)
        {
            return excludedTypes.Contains(propertyInfo.PropertyType) ||
                   excludedMembers.Contains(propertyInfo);
        }

        private bool TryGetSerializer(PropertyInfo propertyInfo, out Func<object, string> serializer)
        {
            if (membersConfig.TryGetValue(propertyInfo, out var config))
            {
                serializer = config.Serializer;
                return true;
            }

            if (typesConfigs.TryGetValue(propertyInfo.PropertyType, out config))
            {
                serializer = config.Serializer;
                return true;
            }

            serializer = null;
            return false;
        }
    }
}