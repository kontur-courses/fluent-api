using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public Dictionary<Type, Func<PropertyInfo, object, string>> serWay;
        public Dictionary<Type, CultureInfo> typesCultures;
        private const string BuiltInScope = "CommonLanguageRuntimeLibrary";

        public PrintingConfig()
        {
            typesCultures = new Dictionary<Type, CultureInfo>();
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
                    sb.AppendLine(identation + propertyInfo.Name + " = " +
                            propertyInfo.GetValue(obj));
                    continue;
                }
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }

            foreach (var fieldInfo in type.GetFields())
            {
                if (fieldInfo.FieldType.Module.ScopeName == BuiltInScope)
                {
                    sb.AppendLine(identation + fieldInfo.Name + " = " +
                        fieldInfo.GetValue(obj));
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