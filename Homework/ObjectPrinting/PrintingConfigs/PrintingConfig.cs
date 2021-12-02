using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public HashSet<Type> ExcludedTypes;
        public HashSet<string> ExcludedNames;
        public Dictionary<Type, Func<object, string>> typeAlterSerializations;
        public Dictionary<Type, Func<object, string>> culturedSerializations;
        public Dictionary<string, Func<object, string>> nameAlterSerializations;
        public Dictionary<string, int> trimLengthByName;
        private const string BuiltInScope = "CommonLanguageRuntimeLibrary";

        public PrintingConfig()
        {
            ExcludedTypes = new HashSet<Type>();
            ExcludedNames = new HashSet<string>();
            culturedSerializations = new Dictionary<Type, Func<object, string>>();
            typeAlterSerializations = new Dictionary<Type, Func<object, string>>();
            nameAlterSerializations = new Dictionary<string, Func<object, string>>();
            trimLengthByName = new Dictionary<string, int>();
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                var propType = propertyInfo.PropertyType;
                var propValue = propertyInfo.GetValue(obj);
                if (IsPropertyExcluded(propertyInfo, propType))
                    continue;
                if (typeAlterSerializations.ContainsKey(propType))
                {
                    var serialize = typeAlterSerializations[propType];
                    var serializedValue = serialize(propValue);
                    if (trimLengthByName.ContainsKey(propertyInfo.Name))
                        serializedValue = serializedValue.Substring(0, trimLengthByName[propertyInfo.Name]);
                    sb.AppendLine(indentation + serializedValue);
                }
                else if (nameAlterSerializations.ContainsKey(propertyInfo.Name))
                {
                    var serialize = nameAlterSerializations[propertyInfo.Name];
                    var serializedValue = serialize(propValue);
                    if (trimLengthByName.ContainsKey(propertyInfo.Name))
                        serializedValue = serializedValue.Substring(0, trimLengthByName[propertyInfo.Name]);
                    sb.AppendLine(indentation + serializedValue);
                }
                else if (propertyInfo.PropertyType.Module.ScopeName == BuiltInScope)
                {
                    if (culturedSerializations.ContainsKey(propType))
                        propValue = culturedSerializations[propType](propValue);
                    var serializedValue = propValue.ToString();
                    if (trimLengthByName.ContainsKey(propertyInfo.Name))
                        serializedValue = serializedValue.Substring(0, trimLengthByName[propertyInfo.Name]);
                    sb.AppendLine(indentation + propertyInfo.Name + " = " + serializedValue);
                }
                else 
                {
                    sb.Append(indentation + propertyInfo.Name + " = " +
                        PrintToString(propValue, nestingLevel + 1));
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

        private bool IsPropertyExcluded(PropertyInfo propertyInfo, Type propType)
        {
            return ExcludedNames.Contains(propertyInfo.Name) || ExcludedTypes.Contains(propType);
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
            var propInfo = ((MemberExpression) memberSelector.Body).Member as PropertyInfo;
            ExcludedNames.Add(propInfo.Name);
            return this;
        }
    }
}