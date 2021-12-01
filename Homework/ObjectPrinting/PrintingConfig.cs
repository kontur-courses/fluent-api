﻿using System;
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
                if (serWay.ContainsKey(propertyInfo.PropertyType))
                {
                    var config = serWay[propertyInfo.PropertyType];
                    var serializatedProperty = config(propertyInfo, obj); 
                    var serializationResult = serializatedProperty == ""
                        ? ""
                        : identation + serializatedProperty;
                    sb.Append(serializationResult);
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

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            serWay[typeof(TPropType)] = (p, o) => ""; 
            return this;
        }
    }
}