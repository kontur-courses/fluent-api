using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public HashSet<Type> ExcludedTypes;
        public HashSet<string> ExcludedNames;
        public Dictionary<object, string> AlreadySerialized;
        public Dictionary<Type, Func<object, string>> typeAlterSerializations;
        public Dictionary<Type, Func<object, string>> culturedSerializations;
        public Dictionary<string, Func<object, string>> nameAlterSerializations;
        public Dictionary<string, int> trimLengthByName;
        private const string BuiltInScope = "CommonLanguageRuntimeLibrary";

        public PrintingConfig()
        {
            ExcludedTypes = new HashSet<Type>();
            ExcludedNames = new HashSet<string>();
            AlreadySerialized = new Dictionary<object, string>();
            culturedSerializations = new Dictionary<Type, Func<object, string>>();
            typeAlterSerializations = new Dictionary<Type, Func<object, string>>();
            nameAlterSerializations = new Dictionary<string, Func<object, string>>();
            trimLengthByName = new Dictionary<string, int>();
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
            ExcludedTypes.Add(typeof(TPropType));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propInfo = ((MemberExpression)memberSelector.Body).Member as PropertyInfo;
            ExcludedNames.Add(propInfo.Name);
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (AlreadySerialized.ContainsKey(obj))
                return AlreadySerialized[obj];

            if (obj == null)
                return "null" + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            AlreadySerialized[obj] = type.Name;
            sb.AppendLine(type.Name);
            foreach (var propInfo in type.GetProperties())
            {
                if (IsPropertyExcluded(propInfo, propInfo.PropertyType))
                    continue;
                if (typeAlterSerializations.ContainsKey(propInfo.PropertyType))
                {
                    sb.AppendLine(indentation + ApplyTypeSerialization(propInfo,
                        propInfo.GetValue(obj)));
                }
                else if (nameAlterSerializations.ContainsKey(propInfo.Name))
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
            return sb.ToString();
        }

        private string ApplyNameSerialization(PropertyInfo propertyInfo, object propValue)
        {
            var serialize = nameAlterSerializations[propertyInfo.Name];
            return ApplyCustomSerialization(propertyInfo, propValue, serialize);
        }

        private string ApplyTypeSerialization(PropertyInfo propertyInfo, object propValue)
        {
            var serialize = typeAlterSerializations[propertyInfo.PropertyType];
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
            if (culturedSerializations.ContainsKey(propType))
                propValue = culturedSerializations[propType](propValue);
            return propValue;
        }

        private string TrimIfPossible(PropertyInfo propertyInfo, string serializedValue)
        {
            if (trimLengthByName.ContainsKey(propertyInfo.Name))
                serializedValue = serializedValue.Substring(0, trimLengthByName[propertyInfo.Name]);
            return serializedValue;
        }

        private bool IsPropertyExcluded(PropertyInfo propertyInfo, Type propType)
        {
            return ExcludedNames.Contains(propertyInfo.Name) || ExcludedTypes.Contains(propType);
        }
    }
}