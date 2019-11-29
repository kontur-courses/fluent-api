using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> : IPrintingConfig
    {
        private readonly HashSet<Type> finalTypes;
        private readonly SerializationInfo serializationInfo;

        public PrintingConfig()
        {
            finalTypes = new HashSet<Type>
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan), typeof(bool), typeof(decimal),
                typeof(byte), typeof(long)
            };
            serializationInfo = new SerializationInfo(finalTypes);
        }

        SerializationInfo IPrintingConfig.SerializationInfo => serializationInfo;

        public PrintingConfig<TOwner> Excluding<T>()
        {
            serializationInfo.ExcludeType(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> func)
        {
            var propInfo = ((MemberExpression) func.Body).Member as PropertyInfo;
            var fieldInfo = ((MemberExpression) func.Body).Member as FieldInfo;
            serializationInfo.ExcludeName(propInfo != null ? propInfo.Name : fieldInfo?.Name);
            return this;
        }

        public PropertySerializationConfig<TOwner, TPropType> For<TPropType>(
            Expression<Func<TOwner, TPropType>> func)
        {
            var propInfo = ((MemberExpression) func.Body).Member as PropertyInfo;
            var fieldInfo = ((MemberExpression) func.Body).Member as FieldInfo;
            return propInfo != null
                ? new PropertySerializationConfig<TOwner, TPropType>(this, propInfo.Name)
                : new PropertySerializationConfig<TOwner, TPropType>(this, fieldInfo?.Name);
        }

        public PropertySerializationConfig<TOwner, TPropType> For<TPropType>()
        {
            return new PropertySerializationConfig<TOwner, TPropType>(this);
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, new HashSet<object>());
        }

        private string PrintToString(object obj, HashSet<object> typeParents)
        {
            if (obj == null)
                return "null" + Environment.NewLine;


            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            foreach (var fieldInfo in type.GetFields()) 
                sb.Append(GetFieldSerialization(fieldInfo, obj, typeParents));

            foreach (var propertyInfo in type.GetProperties())
                sb.Append(GetPropertySerialization(propertyInfo, obj, typeParents));

            sb.Append(Environment.NewLine);
            return sb.ToString();
        }

        public StringBuilder GetFieldSerialization(FieldInfo fieldInfo, object obj,
            HashSet<object> typeParents)
        {
            var currentValue = fieldInfo.GetValue(obj);
            var currentType = fieldInfo.FieldType;
            var currentName = fieldInfo.Name;
            if (typeParents.Contains(fieldInfo))
                return new StringBuilder(
                    $"Circular objects were not serialized: name = {currentName} ,  type = {currentType}"+
                    Environment.NewLine);

            typeParents.Add(fieldInfo);
            var serialization = GetSerialization(currentValue, currentType, currentName, typeParents);
            typeParents.Remove(fieldInfo);
            return serialization;
        }

        public StringBuilder GetPropertySerialization(PropertyInfo propertyInfo, object obj,
            HashSet<object> typeParents)
        {
            var currentValue = propertyInfo.GetValue(obj);
            var currentType = propertyInfo.PropertyType;
            var currentName = propertyInfo.Name;
            if (typeParents.Contains(propertyInfo))
                return new StringBuilder(
                    $"Circular objects were not serialized: name = {currentName} ,  type = {currentType}" +
                    Environment.NewLine);

            typeParents.Add(propertyInfo);
            var serialization = GetSerialization(currentValue, currentType, currentName, typeParents);
            typeParents.Remove(propertyInfo);
            return serialization;
        }


        private StringBuilder GetSerialization(object currentValue, Type currentType, string currentName,
            HashSet<object> typeParents)
        {
            var nestingLevel = typeParents.Count;
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            if (serializationInfo.Excluded(currentType, currentName))
                return sb;

            if (serializationInfo.TryGetSerialization(currentValue, currentType, currentName, identation,
                out var serializedProperty))
            {
                sb.Append(serializedProperty);
                sb.Append(Environment.NewLine);
            }
            else
            {
                sb.Append(identation + currentName + " = " +
                          PrintToString(currentValue,
                              typeParents));
            }

            return sb;
        }
    }
}