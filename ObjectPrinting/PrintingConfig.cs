using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        static readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(int), 
            typeof(double), 
            typeof(float), 
            typeof(string),
            typeof(DateTime), 
            typeof(TimeSpan),
            typeof(Guid)
        };
        private readonly HashSet<Type> excludedTypes;
        private readonly HashSet<PropertyInfo> excludedProperties;
        private readonly HashSet<FieldInfo> excludedFields;
        private readonly Dictionary<Type, Delegate> typeSerializers;
        private readonly Dictionary<PropertyInfo, Delegate> propertySerializers;
        private readonly Dictionary<FieldInfo, Delegate> fieldSerializers;
        private readonly HashSet<object> visited;

        public PrintingConfig()
        {
           excludedTypes = new HashSet<Type>();
           excludedProperties = new HashSet<PropertyInfo>();
           excludedFields = new HashSet<FieldInfo>();
           typeSerializers = new Dictionary<Type, Delegate>();
           propertySerializers = new Dictionary<PropertyInfo, Delegate>();
           fieldSerializers = new Dictionary<FieldInfo, Delegate>();
           visited = new HashSet<object>();
        }

        internal PrintingConfig(PrintingConfig<TOwner> config)
        {
            excludedTypes = config.excludedTypes;
            excludedProperties = config.excludedProperties;
            excludedFields = config.excludedFields;
            typeSerializers = config.typeSerializers;
            propertySerializers = config.propertySerializers;
            fieldSerializers = config.fieldSerializers;
            visited = config.visited;
        }

        internal PrintingConfig(PrintingConfig<TOwner> config,
            Type type,
            Delegate serializer) : this(config)
        {
            typeSerializers.Add(type, serializer);
        }

        internal PrintingConfig(PrintingConfig<TOwner> config,
            PropertyInfo propertyInfo,
            Delegate serializer) : this(config)
        {
            propertySerializers.Add(propertyInfo, serializer);
        }

        internal PrintingConfig(PrintingConfig<TOwner> config,
            FieldInfo fieldInfo,
            Delegate serializer) : this(config)
        {
            fieldSerializers.Add(fieldInfo, serializer);
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;

            visited.Add(obj);
            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);

            //if (obj is IDictionary dictionary)
            //    return PrintDictionary(sb, dictionary, indentation, nestingLevel);
            if (obj is IEnumerable collection)
                return PrintCollection(sb, collection, indentation, nestingLevel);

            PrintProperties(sb, type, obj, indentation, nestingLevel);
            PrintFields(sb, type, obj, indentation, nestingLevel);
            return sb.ToString();
        }

        private string PrintCollection(StringBuilder sb, 
            IEnumerable collection, 
            string indentation,
            int nestingLevel)
        {
            foreach (var item in collection)
            {
                sb.Append(indentation)
                    .Append(PrintToString(item, nestingLevel));
                //.Append("Key = ")
                //.Append(PrintToString(key, nestingLevel))
                //.Append(indentation)
                //.Append("Value = ")
                //.Append(PrintToString(collection[key], nestingLevel));
            }

            return sb.ToString();
        }

        private void PrintFields(StringBuilder sb, Type type, object obj, string indentation, int nestingLevel)
        {
            var fields = type.GetFields()
                .Where(field => !excludedFields.Contains(field)
                                && !excludedTypes.Contains(field.FieldType));
            foreach (var fieldInfo in fields)
            {
                var tabulation = indentation + fieldInfo.Name + " = ";
                if (visited.Contains(fieldInfo.GetValue(obj)))
                    continue;
                sb.Append(tabulation)
                    .Append(PrintField(fieldInfo, obj, nestingLevel));
            }
        }

        private void PrintProperties(StringBuilder sb, Type type, object obj, string indentation, int nestingLevel)
        {
            var properties = type.GetProperties()
                .Where(property => !excludedProperties.Contains(property)
                                && !excludedTypes.Contains(property.PropertyType));
            foreach (var propertyInfo in properties)
            {
                var tabulation = indentation + propertyInfo.Name + " = ";
                if (visited.Contains(propertyInfo.GetValue(obj)))
                    continue;
                sb.Append(tabulation)
                    .Append(PrintProperty(propertyInfo, obj, nestingLevel));
            }
        }

        private string PrintProperty(PropertyInfo property, object obj, int nestingLevel)
        {
            if (typeSerializers.TryGetValue(property.PropertyType, out var serializer))
                return serializer.DynamicInvoke(property.GetValue(obj)) + Environment.NewLine;
            
            return propertySerializers.TryGetValue(property, out serializer)
                ? serializer.DynamicInvoke(property.GetValue(obj)) + Environment.NewLine
                : PrintToString(property.GetValue(obj), nestingLevel + 1);
        }

        private string PrintField(FieldInfo field, object obj, int nestingLevel)
        {
            if (typeSerializers.TryGetValue(field.FieldType, out var serializer))
                return serializer.DynamicInvoke(field.GetValue(obj)) + Environment.NewLine;
            
            return fieldSerializers.TryGetValue(field, out serializer)
                ? serializer.DynamicInvoke(field.GetValue(obj)) + Environment.NewLine
                : PrintToString(field.GetValue(obj), nestingLevel + 1);
        }

        public PrintingConfig<TOwner> Excluding<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<T>(Expression<Func<TOwner, T>> memberSelector)
        {
            if (!(memberSelector.Body is MemberExpression memberExpr))
                throw new ArgumentException($"This is not member expression {memberSelector}");
            var propertyInfo = memberExpr.Member as PropertyInfo;
            if (propertyInfo != null)
                excludedProperties.Add(propertyInfo);
            else if (memberExpr.Member is FieldInfo fieldInfo)
                excludedFields.Add(fieldInfo);
            else
                throw new ArgumentException($"Wrong member selector {memberExpr}");
            return this;
        }

        public IMemberPrintingConfig<TOwner, T> Printing<T>()
        {
            return new TypePrintingConfig<TOwner, T>(this);
        }

        public IMemberPrintingConfig<TOwner, T> Printing<T>(Expression<Func<TOwner, T>> memberSelector)
        {
            if (!(memberSelector.Body is MemberExpression memberExpr))
                throw new ArgumentException($"This is not member expression {memberSelector}");
            var propertyInfo = memberExpr.Member as PropertyInfo;
            var fieldInfo = memberExpr.Member as FieldInfo;
            if (propertyInfo == null && fieldInfo == null)
                throw new ArgumentException($"Wrong member selector {memberExpr}");
            return propertyInfo == null 
                ? (IMemberPrintingConfig<TOwner, T>) new FieldPrintingConfig<TOwner, T>(this, fieldInfo)
                : new PropertyPrintingConfig<TOwner, T>(this, propertyInfo);
        }
    }
}