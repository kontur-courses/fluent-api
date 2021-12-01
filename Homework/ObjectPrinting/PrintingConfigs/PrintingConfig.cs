using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public Dictionary<Type, Func<object, string>> alternativeSerializations;
        public Dictionary<Type, Func<object, string>> culturedSerializations;
        private const string BuiltInScope = "CommonLanguageRuntimeLibrary";

        public PrintingConfig()
        {
            culturedSerializations = new Dictionary<Type, Func<object, string>>();
            alternativeSerializations = new Dictionary<Type, Func<object, string>>();
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
                if (alternativeSerializations.ContainsKey(propType))
                {
                    var serialize = alternativeSerializations[propType];
                    var serializatedProperty = serialize(propValue);
                    var line = GetSerializatedLine(indentation, serializatedProperty);
                    sb.Append(line);
                }
                else if (propertyInfo.PropertyType.Module.ScopeName == BuiltInScope)
                {
                    if (culturedSerializations.ContainsKey(propType))
                        propValue = culturedSerializations[propType](propValue);
                    sb.AppendLine(indentation + propertyInfo.Name + " = " + propValue);
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

        private static string GetSerializatedLine(string indentation, string serializatedProperty)
        {
            return serializatedProperty == ""
                ? serializatedProperty
                : indentation + serializatedProperty;
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            alternativeSerializations[typeof(TPropType)] = value => ""; 
            return this;
        }
    }
}