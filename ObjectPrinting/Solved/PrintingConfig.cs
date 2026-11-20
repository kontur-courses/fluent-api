using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Solved
{
    public class PrintingConfig<TOwner>
    {
        private HashSet<Type> _excludeTypes = new();
        private HashSet<PropertyInfo> _excludeProperties = new();
        private Dictionary<Type, Func<object, string>> _typeSerializersDict = new();
        private Dictionary<PropertyInfo, Func<object, string>> _propertySerializersDict = new();

        //serializeType
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        //serializeProp
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyInfo = (PropertyInfo)((MemberExpression)memberSelector.Body).Member;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyInfo);
        }

        //excludeProperty
        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyInfo = (PropertyInfo)((MemberExpression)memberSelector.Body).Member;
            _excludeProperties.Add(propertyInfo);
            return this;
        }

        //excludeType
        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            _excludeTypes.Add(typeof(TPropType));
            return this;
        }

        public void SetTypeSerialize<TPropType>(Func<TPropType, string> print)
        {
            _typeSerializersDict[typeof(TPropType)] = x => print((TPropType)x);
        }

        public void SetPropertySerialize<TPropType>(PropertyInfo property, Func<TPropType, string> print)
        {
            _propertySerializersDict[property] = x => print((TPropType)x);
        }

        public string PrintToString(TOwner obj)
        {
            var sb = new StringBuilder();
            var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
            AppendValue(sb, obj, nestingLevel: 0, visited);
            return sb.ToString();
        }

        private static readonly HashSet<Type> FinalTypes = new()
        {
            typeof(string), typeof(decimal),
            typeof(DateTime), typeof(DateTimeOffset),
            typeof(TimeSpan), typeof(Guid),
        };

        private static bool IsFinalType(Type type) =>
            type.IsPrimitive || type.IsEnum || FinalTypes.Contains(type);

        private void AppendValue(StringBuilder sb, object? value, int nestingLevel, HashSet<object> visited)
        {
            if (value is null) 
            { 
                sb.Append("is null"); 
                return; 
            }

            var type = value.GetType();
            if (IsFinalType(type)) 
            { 
                sb.Append(value); 
                return; 
            }

            if (!type.IsValueType && !visited.Add(value))
            {
                sb.Append("is cycle");
                return;
            }

            if (value is IDictionary dict) SerializeDictionary(sb, dict, nestingLevel, visited);
            else if (value is IEnumerable seq) SerializeEnumerable(sb, seq, nestingLevel, visited);
            else SerializeObject(sb, value, type, nestingLevel, visited);

            if (!type.IsValueType) visited.Remove(value);
        }

        private void SerializeObject(StringBuilder sb, object obj, Type type, int nestingLevel, HashSet<object> visited)
        {
            var memberIndent = new string('\t', nestingLevel + 1);
            var closeIndent = new string('\t', nestingLevel);
            var nl = Environment.NewLine;

            sb.Append(type.Name).Append(" {").Append(nl);

            foreach (var property in type.GetProperties())
            {
                if (_excludeTypes.Contains(property.PropertyType)) continue;
                if (_excludeProperties.Contains(property)) continue;

                sb.Append(memberIndent).Append(property.Name).Append(" = ");
                SerializeMemberValue(sb, property, property.GetValue(obj), nestingLevel + 1, visited);
                sb.Append(nl);
            }

            sb.Append(closeIndent).Append('}');
        }

        private void SerializeMemberValue(StringBuilder sb, PropertyInfo property, object? value, int nestingLevel, HashSet<object> visited)
        {
            if (_propertySerializersDict.TryGetValue(property, out var propSerializer))
                sb.Append(propSerializer(value!));

            else if (_typeSerializersDict.TryGetValue(property.PropertyType, out var typeSerializer))
                sb.Append(typeSerializer(value!));

            else 
                AppendValue(sb, value, nestingLevel, visited);
        }

        private void SerializeEnumerable(StringBuilder sb, IEnumerable enumerable, int nestingLevel, HashSet<object> visited)
        {
            var itemIndent = new string('\t', nestingLevel + 1);
            var closeIndent = new string('\t', nestingLevel);
            var nl = Environment.NewLine;

            sb.Append('[').Append(nl);
            var i = 0;
            foreach (var item in enumerable)
            {
                sb.Append(itemIndent).Append('[').Append(i++).Append("] = ");
                AppendValue(sb, item, nestingLevel + 1, visited);
                sb.Append(nl);
            }
            sb.Append(closeIndent).Append(']');
        }

        private void SerializeDictionary(StringBuilder sb, IDictionary dict, int nestingLevel, HashSet<object> visited)
        {
            var itemIndent = new string('\t', nestingLevel + 1);
            var closeIndent = new string('\t', nestingLevel);
            var nl = Environment.NewLine;

            sb.Append('{').Append(nl);
            foreach (DictionaryEntry entry in dict)
            {
                sb.Append(itemIndent);
                AppendValue(sb, entry.Key, nestingLevel + 1, visited);
                sb.Append(" = ");
                AppendValue(sb, entry.Value, nestingLevel + 1, visited);
                sb.Append(nl);
            }
            sb.Append(closeIndent).Append('}');
        }
    }
}