using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class ObjectSerializer<TOwner>
    {
        private readonly HashSet<object> visited;
        private readonly IPrintingConfig<TOwner> config;
        private readonly HashSet<Type> finalTypes = new HashSet<Type>()
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
        };

        public ObjectSerializer(IPrintingConfig<TOwner> printingConfig)
        {
            config = printingConfig;
            visited = new HashSet<object>();
        }

        public string Serialize(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            if (visited.Contains(obj))
                return "cycle" + Environment.NewLine;
            visited.Add(obj);

            if (config.TypeSerialization.TryGetValue(type, out var serializer))
                return serializer.DynamicInvoke(obj).ToString() + Environment.NewLine; ;

            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var serializedObj = new StringBuilder();
            serializedObj.AppendLine(type.Name);
            PropertiesSerialize(serializedObj, type, obj, identation, nestingLevel);
            FieldsSerialize(serializedObj, type, obj, identation, nestingLevel);
            return serializedObj.ToString();
        }

        private void PropertiesSerialize(StringBuilder serializedObj, Type type, object obj,
            string identation, int nestingLevel)
        {
            foreach (var propertyInfo in type.GetProperties()
                .Where(info => !config.ExcludedProperties.Contains(info)
                            && !config.ExcludedTypes.Contains(info.PropertyType)))
            {
                serializedObj.Append(identation + propertyInfo.Name + " = " +
                          PrintProperty(propertyInfo.GetValue(obj),
                                propertyInfo,
                                nestingLevel + 1));
            }
        }

        private void FieldsSerialize(StringBuilder serializedObj, Type type, object obj,
            string identation, int nestingLevel)
        {
            foreach (var fieldInfo in type.GetFields()
                .Where(info => !config.ExcludedFields.Contains(info)
                            && !config.ExcludedTypes.Contains(info.FieldType)))
            {
                serializedObj.Append(identation + fieldInfo.Name + " = " +
                          PrintField(fieldInfo.GetValue(obj),
                                fieldInfo,
                                nestingLevel + 1));
            }
        }

        private string PrintProperty(object value, PropertyInfo propertyInfo, int nestingLevel)
        {
            return config.PropertySerialization.TryGetValue(propertyInfo, out var serializer) ?
                 serializer.DynamicInvoke(value).ToString() + Environment.NewLine :
                 Serialize(value, nestingLevel);
        }

        private string PrintField(object value, FieldInfo fieldInfo, int nestingLevel)
        {
            return config.FieldSerialization.TryGetValue(fieldInfo, out var serializer) ?
                 serializer.DynamicInvoke(value).ToString() + Environment.NewLine :
                 Serialize(value, nestingLevel);
        }
    }
}
