using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> _excludedTypes;
        private readonly HashSet<string> _excludedNames;
        public readonly Dictionary<object, string> AlreadySerialized;
        public readonly Dictionary<Type, Func<object, string>> TypeAlterSerializations;
        public readonly Dictionary<Type, Func<object, string>> CulturedSerializations;
        public readonly Dictionary<string, Func<object, string>> NameAlterSerializations;
        public readonly Dictionary<string, int> TrimLengthByName;
        private const string BuiltInScope = "CommonLanguageRuntimeLibrary";

        public PrintingConfig()
        {
            _excludedTypes = new HashSet<Type>();
            _excludedNames = new HashSet<string>();
            AlreadySerialized = new Dictionary<object, string>();
            CulturedSerializations = new Dictionary<Type, Func<object, string>>();
            TypeAlterSerializations = new Dictionary<Type, Func<object, string>>();
            NameAlterSerializations = new Dictionary<string, Func<object, string>>();
            TrimLengthByName = new Dictionary<string, int>();
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propInfo = ((MemberExpression)memberSelector.Body).Member as PropertyInfo;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propInfo);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            _excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propInfo = ((MemberExpression)memberSelector.Body).Member as PropertyInfo;
            _excludedNames.Add(propInfo.Name);
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (AlreadySerialized.ContainsKey(obj))
                return AlreadySerialized[obj];

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            AlreadySerialized[obj] = type.Name;
            sb.AppendLine(type.Name);
            SerializeProperties(obj, nestingLevel, type, sb, indentation);
            SerializeFields(obj, nestingLevel, type, sb, indentation);
            return sb.ToString();
        }

        private void SerializeFields(
            object obj, 
            int nestingLevel, 
            Type type, 
            StringBuilder sb, 
            string indentation)
        {
            foreach (var fieldInfo in type.GetFields())
            {
                if (fieldInfo.FieldType.Module.ScopeName == BuiltInScope)
                {
                    sb.AppendLine(indentation + fieldInfo.Name + " = " +
                                  fieldInfo.GetValue(obj));
                    continue;
                }

                sb.Append(indentation + fieldInfo.Name + " = " +
                          PrintToString(fieldInfo.GetValue(obj),
                              nestingLevel + 1));
            }
        }

        private void SerializeProperties(
            object obj, 
            int nestingLevel, 
            Type type, 
            StringBuilder sb, 
            string indentation)
        {
            foreach (var propInfo in type.GetProperties())
            {
                if (IsPropertyExcluded(propInfo, propInfo.PropertyType))
                    continue;
                if (TypeAlterSerializations.ContainsKey(propInfo.PropertyType))
                {
                    sb.AppendLine(indentation + ApplyTypeSerialization(propInfo,
                        propInfo.GetValue(obj)));
                }
                else if (NameAlterSerializations.ContainsKey(propInfo.Name))
                {
                    sb.AppendLine(indentation + ApplyNameSerialization(propInfo,
                        propInfo.GetValue(obj)));
                }
                else if (propInfo.PropertyType.Module.ScopeName == BuiltInScope)
                {
                    var propValue = ApplyCultureIfPossible(propInfo.PropertyType, propInfo.GetValue(obj));
                    var serializedValue = propValue.ToString();
                    serializedValue = TrimIfPossible(propInfo, serializedValue);
                    sb.AppendLine(indentation + propInfo.Name + " = " + serializedValue);
                }
                else
                {
                    sb.Append(indentation + propInfo.Name + " = " +
                              PrintToString(propInfo.GetValue(obj), nestingLevel + 1));
                }
            }
        }

        private string ApplyNameSerialization(PropertyInfo propertyInfo, object propValue)
        {
            var serialize = NameAlterSerializations[propertyInfo.Name];
            return ApplyCustomSerialization(propertyInfo, propValue, serialize);
        }

        private string ApplyTypeSerialization(PropertyInfo propertyInfo, object propValue)
        {
            var serialize = TypeAlterSerializations[propertyInfo.PropertyType];
            return ApplyCustomSerialization(propertyInfo, propValue, serialize);
        }

        private string ApplyCustomSerialization(PropertyInfo propertyInfo, object propValue,
            Func<object, string> serialize)
        {
            var serializedValue = serialize(propValue);
            return TrimIfPossible(propertyInfo, serializedValue);
        }

        private object ApplyCultureIfPossible(Type propType, object propValue)
        {
            if (CulturedSerializations.ContainsKey(propType))
                propValue = CulturedSerializations[propType](propValue);
            return propValue;
        }

        private string TrimIfPossible(PropertyInfo propertyInfo, string serializedValue)
        {
            if (TrimLengthByName.ContainsKey(propertyInfo.Name))
                serializedValue = serializedValue.Substring(0, TrimLengthByName[propertyInfo.Name]);
            return serializedValue;
        }

        private bool IsPropertyExcluded(PropertyInfo propertyInfo, Type propType)
        {
            return _excludedNames.Contains(propertyInfo.Name) || _excludedTypes.Contains(propType);
        }
    }
}