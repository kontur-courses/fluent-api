using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes;
        private readonly Dictionary<Type, Delegate> typeSerialization;
        private readonly Dictionary<PropertyInfo, Delegate> propertySerialization;
        private readonly Dictionary<FieldInfo, Delegate> fieldSerialization;
        private readonly HashSet<PropertyInfo> excludedProperties;
        private readonly HashSet<FieldInfo> excludedFields;
        private readonly HashSet<object> visited;

        #region IPrintingConfig Property Init
        HashSet<Type> IPrintingConfig<TOwner>.ExcludedTypes => excludedTypes;

        Dictionary<Type, Delegate> IPrintingConfig<TOwner>.TypeSerialization => typeSerialization;

        Dictionary<PropertyInfo, Delegate> IPrintingConfig<TOwner>.PropertySerialization => propertySerialization;

        HashSet<PropertyInfo> IPrintingConfig<TOwner>.ExcludedProperties => excludedProperties;
        HashSet<FieldInfo> IPrintingConfig<TOwner>.ExcludedFields => excludedFields;
        Dictionary<FieldInfo, Delegate> IPrintingConfig<TOwner>.FieldSerialization => fieldSerialization;
        #endregion

        public PrintingConfig()
        {
            excludedTypes = new HashSet<Type>();
            excludedProperties = new HashSet<PropertyInfo>();
            visited = new HashSet<object>();
            typeSerialization = new Dictionary<Type, Delegate>();
            propertySerialization = new Dictionary<PropertyInfo, Delegate>();
            excludedFields = new HashSet<FieldInfo>();
            fieldSerialization = new Dictionary<FieldInfo, Delegate>();
        }

        public string PrintToString(TOwner obj)
        {
            var objToString = PrintToString(obj, 0);
            visited.Clear();
            return objToString;
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            var type = obj.GetType();
            if (visited.Contains(obj))
                return "cycle" + Environment.NewLine;
            visited.Add(obj);

            if (typeSerialization.TryGetValue(type, out var serializer))
                return serializer.DynamicInvoke(obj).ToString();

            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
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
                .Where(info => !excludedProperties.Contains(info) && !excludedTypes.Contains(info.PropertyType)))
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
                .Where(info => !excludedFields.Contains(info) && !excludedTypes.Contains(info.FieldType)))
            {
                serializedObj.Append(identation + fieldInfo.Name + " = " +
                          PrintField(fieldInfo.GetValue(obj),
                                fieldInfo,
                                nestingLevel + 1));
            }
        }

        private string PrintProperty(object value, PropertyInfo propertyInfo, int nestingLevel)
        {
            return propertySerialization.TryGetValue(propertyInfo, out var serializer) ?
                 serializer.DynamicInvoke(value).ToString() + Environment.NewLine :
                 PrintToString(value, nestingLevel);
        }

        private string PrintField(object value, FieldInfo fieldInfo, int nestingLevel)
        {
            return fieldSerialization.TryGetValue(fieldInfo, out var serializer) ?
                 serializer.DynamicInvoke(value).ToString() + Environment.NewLine :
                 PrintToString(value, nestingLevel);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyInfo = ((MemberExpression)memberSelector.Body).Member as PropertyInfo;
            var fieldInfo = ((MemberExpression)memberSelector.Body).Member as FieldInfo;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyInfo);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludedProperties.Add(((MemberExpression)memberSelector.Body).Member as PropertyInfo);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }
    }
}