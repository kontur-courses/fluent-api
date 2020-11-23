using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ObjectPrinting.Config;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig<TOwner>
    {
        private static readonly IEnumerable<Type> finalTypes = new[]{
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private readonly HashSet<object> visited = new HashSet<object>();

        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<PropertyInfo> excludedProperties = new HashSet<PropertyInfo>();
        private readonly HashSet<FieldInfo> excludedFields = new HashSet<FieldInfo>();
        private readonly Dictionary<Type, Delegate> typeToSerializer = new Dictionary<Type, Delegate>();
        private readonly Dictionary<PropertyInfo, Delegate> propertyToSerializer = new Dictionary<PropertyInfo, Delegate>();
        private readonly Dictionary<FieldInfo, Delegate> fieldToSerializer = new Dictionary<FieldInfo, Delegate>();

        HashSet<Type> IPrintingConfig<TOwner>.ExcludedTypes => excludedTypes;
        HashSet<PropertyInfo> IPrintingConfig<TOwner>.ExcludedProperties => excludedProperties;
        HashSet<FieldInfo> IPrintingConfig<TOwner>.ExcludedFields => excludedFields;
        Dictionary<Type, Delegate> IPrintingConfig<TOwner>.TypeToSerializer => typeToSerializer;
        Dictionary<PropertyInfo, Delegate> IPrintingConfig<TOwner>.PropertyToSerializer => propertyToSerializer;
        Dictionary<FieldInfo, Delegate> IPrintingConfig<TOwner>.FieldToSerializer => fieldToSerializer;

        public IConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this);
        }

        public IConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyInfo = ((MemberExpression)memberSelector.Body).Member as PropertyInfo;
            var fieldInfo = ((MemberExpression)memberSelector.Body).Member as FieldInfo;
            ThrowIfNotValidMember(propertyInfo, fieldInfo);
            return propertyInfo != null
                ? (IConfig<TOwner, TPropType>)new PropertyPrintingConfig<TOwner, TPropType>(this, propertyInfo)
                : new FieldPrintingConfig<TOwner, TPropType>(this, fieldInfo);
        }

        public IPrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyInfo = ((MemberExpression)memberSelector.Body).Member as PropertyInfo;
            var fieldInfo = ((MemberExpression)memberSelector.Body).Member as FieldInfo;
            ThrowIfNotValidMember(propertyInfo, fieldInfo);
            _ = propertyInfo != null
                ? excludedProperties.Add(propertyInfo)
                : excludedFields.Add(fieldInfo);
            return this;
        }

        public IPrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
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

            var type = obj.GetType();
            if (visited.Contains(obj))
                return "cycle" + Environment.NewLine;

            if (typeToSerializer.TryGetValue(type, out var serializer))
                return serializer.DynamicInvoke(obj) + Environment.NewLine;

            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;

            if (obj is IEnumerable enumerable)
                return PrintCollection(enumerable, nestingLevel) + Environment.NewLine;

            visited.Add(obj);

            var indent = GetIndent(nestingLevel);
            var serializedObj = new StringBuilder()
                .AppendLine(type.Name)
                .Append(SerializeProperties(obj, type, indent, nestingLevel))
                .Append(SerializeFields(obj, type, indent, nestingLevel));
            return serializedObj.ToString();
        }

        private static string GetIndent(int nestingLevel)
        {
            return new string('\t', nestingLevel + 1);
        }

        private StringBuilder SerializeProperties(object obj, Type type, string indent, int nestingLevel)
        {
            var sb = new StringBuilder();

            foreach (var propInfo in type.GetProperties()
                .Where(info => !excludedProperties.Contains(info) &&
                               !excludedTypes.Contains(info.PropertyType)))
                sb.Append(indent)
                    .Append(propInfo.Name)
                    .Append(" = ")
                    .Append(PrintProperty(propInfo.GetValue(obj), propInfo, nestingLevel + 1));

            return sb;
        }

        private StringBuilder SerializeFields(object obj, Type type, string indent, int nestingLevel)
        {
            var sb = new StringBuilder();

            foreach (var fieldInfo in type.GetFields()
                .Where(info => !excludedFields.Contains(info) &&
                               !excludedTypes.Contains(info.FieldType)))
                sb.Append(indent)
                    .Append(fieldInfo.Name)
                    .Append(" = ")
                    .Append(PrintField(fieldInfo.GetValue(obj), fieldInfo, nestingLevel + 1));

            return sb;
        }

        private string PrintProperty(object value, PropertyInfo propertyInfo, int nestingLevel)
        {
            return propertyToSerializer.TryGetValue(propertyInfo, out var serializer) ?
                 serializer.DynamicInvoke(value) + Environment.NewLine :
                 PrintToString(value, nestingLevel);
        }

        private string PrintField(object value, FieldInfo fieldInfo, int nestingLevel)
        {
            return fieldToSerializer.TryGetValue(fieldInfo, out var serializer) ?
                 serializer.DynamicInvoke(value) + Environment.NewLine :
                 PrintToString(value, nestingLevel);
        }

        private string PrintCollection(IEnumerable collection, int nestingLevel)
        {
            var serializedObj = new StringBuilder("[" + Environment.NewLine);
            foreach (var el in collection)
                serializedObj
                    .Append(GetIndent(nestingLevel + 1))
                    .Append(PrintToString(el, nestingLevel + 1))
                    .Append(GetIndent(nestingLevel + 2))
                    .Append("," + Environment.NewLine);

            return serializedObj
                .Remove(serializedObj.Length - 3, 3)
                .Append("]")
                .ToString();
        }

        private static void ThrowIfNotValidMember(PropertyInfo propertyInfo, FieldInfo fieldInfo)
        {
            if (propertyInfo == null && fieldInfo == null)
                throw new ArgumentException("Member selector should define properties or fields");
        }
    }
}