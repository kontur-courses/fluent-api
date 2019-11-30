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
        private static string newLine = Environment.NewLine;

        private static readonly List<Type> finalTypes = new List<Type>
        {
            typeof(string),
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(Guid)
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
            Print(objectToPrint, 0);

            return output.ToString();
        }

        private void Print(object objectToPrint, int nestingLevel, string outputAdd = null)
        {
            var type = objectToPrint.GetType();

            if (typesConfigs.TryGetValue(type, out var config))
            {
                var serialize = config.Serialize;
                if (outputAdd != null)
                    output.Append(outputAdd);
                output.Append(serialize(objectToPrint));
            }
            else if (finalTypes.Contains(type) || type.IsPrimitive)
            {
                if (outputAdd != null)
                    output.Append(outputAdd);

                output.Append(objectToPrint);
            }
            else if (objectToPrint is IEnumerable enumerable)
            {
                if (outputAdd != null)
                    output.Append(outputAdd);

                printed.Add(objectToPrint);
                PrintCollectionValue(enumerable, nestingLevel == -1 ? 0 : nestingLevel);
            }
            else
            {
                if (outputAdd != null)
                    output.Append(outputAdd);

                printed.Add(objectToPrint);
                PrintRegularObjectValue(objectToPrint, nestingLevel);
            }
        }
        
        private void PrintCollectionValue(IEnumerable enumerable, int nestingLevel)
        {
            if (ElementsIsExcluded(enumerable))
            {
                output.Append("[]");
                return;
            }


            output.Append("[").Append(newLine);
            var i = 0;

            foreach (var obj in enumerable)
            {
                if (i != 0)
                {
                    output.Append(",").Append(newLine);
                }

                if (!TryPrint(obj, nestingLevel))
                    break;

                i++;
            }

            output.Append(newLine).Append("]").Append(newLine);
        }

        private void PrintRegularObjectValue(object value, int nestingLevel)
        {
            var indent = new string('\t', nestingLevel);

            var properties = value.GetType().GetProperties().Where(p => !IsExcluded(p)).ToList();

            for (var i = 0; i < properties.Count; i++)
            {
                var property = properties[i];

                if (property.GetValue(value) == null)
                {
                    output.Append(indent).Append(property.Name).Append(" = ").Append("null");
                    continue;
                }

                var propertyValue = property.GetValue(value);

                if (TryGetSerialize(property, out var serialize))
                {
                    output.Append(indent).Append(property.Name).Append(" = ").Append(serialize(propertyValue));
                }
                else
                {
                    var isFinalType = TypesIsFinal(propertyValue);
                    var propertyValueIsCollection = propertyValue is IEnumerable;
                    var valueIsCollection = value is IEnumerable;
                    var end = ((isFinalType && !valueIsCollection) || propertyValueIsCollection)
                        ? " = "
                        : Environment.NewLine;

                    output.Append(indent).Append(property.Name);
                    TryPrint(propertyValue, isFinalType ? nestingLevel : nestingLevel + 1, end);
                }

                if (i != properties.Count - 1)
                    output.Append(newLine);
            }
        }

        private bool TryPrint(object value, int nestingLevel, string outputAdd = null)
        {
            var isReferenceType = !value.GetType().IsValueType;
            if (isReferenceType && printed.Contains(value))
            {
                if (outputAdd != null && outputAdd != newLine)
                {
                    output.Append(outputAdd);
                }

                output.Append("[Cyclic reference detected]");
                return false;
            }

            if (isReferenceType)
                printed.Add(value);

            Print(value, nestingLevel, outputAdd);
            return true;
        }

        private bool ElementsIsExcluded(IEnumerable enumerable)
        {
            var enumerator = enumerable.GetEnumerator();
            enumerator.MoveNext();

            return enumerator.Current != null && excludedTypes.Contains(enumerator.Current.GetType());
        }

        private static bool TypesIsFinal(object info)
        {
            var type = info.GetType();
            return type.IsPrimitive || finalTypes.Contains(type);
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