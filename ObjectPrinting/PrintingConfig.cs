using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner> :IPrintingConfig
    {
        private HashSet<string> notSerializingFieldsAndProperies;
        private HashSet<Type> notSerializingTypes;
        private Dictionary<string, Delegate> Serializers;
        private Dictionary<string, CultureInfo> Cultures;
        private Dictionary<string, int> TrimLenghts;

        public static readonly HashSet<Type> FinalTypes = new HashSet<Type>()
        {
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(short),
            typeof(ushort),
            typeof(sbyte),
            typeof(byte),
            typeof(double),
            typeof(float),
            typeof(decimal),
            typeof(string),
            typeof(DateTime),
            typeof(TimeSpan),
        };

        Dictionary<string, Delegate> IPrintingConfig.Serializers => Serializers;
        Dictionary<string, CultureInfo> IPrintingConfig.Cultures => Cultures;
        Dictionary<string, int> IPrintingConfig.TrimLenghts => TrimLenghts;

        public PrintingConfig()
        {
            notSerializingFieldsAndProperies = new HashSet<string>();
            notSerializingTypes = new HashSet<Type>();
            Serializers = new Dictionary<string, Delegate>();
            TrimLenghts = new Dictionary<string, int>();
            Cultures = new Dictionary<string, CultureInfo>();
        }

        public PrintingConfig<TOwner> Exclude<TPropType>()
        {
            notSerializingTypes.Add(typeof(TPropType));
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TPropType>(Expression<Func<TOwner, TPropType>> propertySelector)
        {
            var propertyName = GetName(propertySelector);
            notSerializingFieldsAndProperies.Add(propertyName);
            return this;
        }

        public TypePrintingConfig<TOwner, TPropType> Serialise<TPropType>()
        {
            return new TypePrintingConfig<TOwner, TPropType>(this, typeof(TPropType));
        }

        public TypePrintingConfig<TOwner, TPropType> Serialise<TPropType>(Expression<Func<TOwner, TPropType>> selector) 
        {
            return new TypePrintingConfig<TOwner, TPropType>(this, GetName(selector));
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 1);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;         
            if (FinalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;
            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            SerializeProperties(sb, identation, obj, nestingLevel);
            sb.Append(Environment.NewLine);
            SerializeFields(sb, identation, obj, nestingLevel);
            return sb.ToString();
        }

        private void SerializeFields(StringBuilder sb,
            string identation, object obj, int nestingLevel)
        {
            var type = obj.GetType();
            foreach (var field in type.GetFields())
            {
                if (field.IsStatic) continue;
                var fieldName = type.FullName + field.Name;
                if (notSerializingFieldsAndProperies.Contains(fieldName)) continue;
                if (notSerializingTypes.Contains(field.FieldType)) continue;
                sb.Append(identation + field.Name + " = ");
                var serializedProperty = TrySerializeFieldInfo(field, obj);
                if (serializedProperty != null)
                    sb.Append(serializedProperty);
                else
                    sb.Append(PrintToString(field.GetValue(obj),
                        nestingLevel + 1));
            }
        }


        private void SerializeProperties(StringBuilder sb, 
            string identation, object obj, int nestingLevel)
        {
            var type = obj.GetType();
            foreach (var property in type.GetProperties())
            {
                var propertyName = type.FullName + property.Name;
                if (notSerializingFieldsAndProperies.Contains(propertyName)) continue;
                if (notSerializingTypes.Contains(property.PropertyType)) continue;
                sb.Append(identation + property.Name + " = ");
                var serealizedProp = TrySerializePropertyInfo(property, obj);
                if (serealizedProp == null)
                {
                    sb.Append(PrintToString(property.GetValue(obj),
                        nestingLevel + 1));
                }
                else
                    sb.Append(serealizedProp);
            }
        }

        private string TrySerializeFieldInfo(FieldInfo fieldInfo, object obj)
        {
            var fieldName = obj.GetType().FullName + fieldInfo.Name;
            var typeName = fieldInfo.FieldType.FullName;
            if (Serializers.ContainsKey(fieldName))
                return Serializers[fieldName]
                    .DynamicInvoke(fieldInfo.GetValue(obj)).ToString();
            if (Serializers.ContainsKey(typeName))
                return Serializers[typeName]
                    .DynamicInvoke(fieldInfo.GetValue(obj)).ToString();
            if (Cultures.ContainsKey(typeName))
            {
                var culture = Cultures[typeName];
                var tmp = fieldInfo.GetValue(obj);
                return string.Format(culture, "{0:F}", tmp);
            }
            if (TrimLenghts.ContainsKey(fieldName))
            {
                var trimLenght = TrimLenghts[fieldName];
                return fieldInfo.GetValue(obj)
                    .ToString()
                    .Substring(0, trimLenght);
            }
            return null;
        }

        private string TrySerializePropertyInfo(PropertyInfo propertyInfo, object obj)
        {
            var propertyName = obj.GetType().FullName + propertyInfo.Name;
            var typeName = propertyInfo.PropertyType.FullName;
            if (Serializers.ContainsKey(propertyName))
                return Serializers[propertyName]
                    .DynamicInvoke(propertyInfo.GetValue(obj)).ToString();
            if (Serializers.ContainsKey(typeName))
                return Serializers[typeName]
                    .DynamicInvoke(propertyInfo.GetValue(obj)).ToString();
            if (Cultures.ContainsKey(typeName))
            {
                var culture = Cultures[typeName];
                var tmp = propertyInfo.GetValue(obj);
                return string.Format(culture, "{0:F}", tmp);
            }
            if (TrimLenghts.ContainsKey(propertyName))
            {
                var trimLenght = TrimLenghts[propertyName];
                return propertyInfo.GetValue(obj)
                    .ToString()
                    .Substring(0, trimLenght);
            }
            return null;
        }

        private string GetName<TOwner, TPropType>(Expression<Func<TOwner, TPropType>> selector)
        {
            var isCorrectProperty = typeof(TOwner)
                .GetProperties()
                .Select(property => property.Name)
                .ToArray().Contains(((MemberExpression)selector.Body).Member.Name);
            var isCorrectField = typeof(TOwner)
                .GetFields()
                .Select(field => field.Name)
                .ToArray().Contains(((MemberExpression)selector.Body).Member.Name);

            if (!isCorrectProperty &&
                !isCorrectField)
                throw new ArgumentException("lambda doesn`t return property or field");
            var name = typeof(TOwner).ToString();
            name += ((MemberExpression)selector.Body).Member.Name;
            return name;
        }
    }
}