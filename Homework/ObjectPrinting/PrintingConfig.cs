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
        private readonly HashSet<Type> _excludedPropertiesTypes;
        private readonly HashSet<Type> _excludedFieldsTypes;
        public Dictionary<Type, Func<PropertyInfo, object, string>> serWay;
        private const string BuiltInScope = "CommonLanguageRuntimeLibrary";

        public PrintingConfig()
        {
            _excludedPropertiesTypes = new HashSet<Type>();
            _excludedFieldsTypes = new HashSet<Type>();
            serWay = new Dictionary<Type, Func<PropertyInfo, object, string>>();
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (_excludedPropertiesTypes.Contains(propertyInfo.PropertyType))
                    continue;
                if (serWay.ContainsKey(propertyInfo.PropertyType))
                {
                    var config = serWay[propertyInfo.PropertyType];
                    sb.Append(identation + config(propertyInfo, obj));
                    continue;
                }
                if (propertyInfo.PropertyType.Module.ScopeName == BuiltInScope)
                {
                    sb.Append(identation + propertyInfo.Name + " = " +
                            propertyInfo.GetValue(obj) + Environment.NewLine);
                    continue;
                }
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            // дублирование 
            foreach (var fieldInfo in type.GetFields())
            {
                if (_excludedFieldsTypes.Contains(fieldInfo.FieldType))
                    continue;
                if (fieldInfo.FieldType.Module.ScopeName == BuiltInScope)
                {
                    sb.Append(identation + fieldInfo.Name + " = " +
                        fieldInfo.GetValue(obj) + Environment.NewLine);
                    continue;
                }
                sb.Append(identation + fieldInfo.Name + " = " +
                          PrintToString(fieldInfo.GetValue(obj),
                              nestingLevel + 1));
            }
            return sb.ToString();
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PrintingConfig<TOwner> ExcludeFieldsTypes(params Type[] types)
        {
            ExcludeTypes(_excludedFieldsTypes, types);
            return this;
        }

        public PrintingConfig<TOwner> ExcludePropertiesTypes(params Type[] types)
        {
            ExcludeTypes(_excludedPropertiesTypes, types);
            return this;
        }

        private void ExcludeTypes(HashSet<Type> excludedTypes, params Type[] types)
        {
            foreach (var type in types) excludedTypes.Add(type);
        }
    }
}