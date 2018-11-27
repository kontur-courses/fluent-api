using System;
using System.Collections;
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
        private readonly IEnumerable<Type> baseTypes = new List<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private readonly Dictionary<string, int> cutProperty = new Dictionary<string, int>();

        private readonly List<string> excludingProperty = new List<string>();
        private readonly List<Type> excludingTypes = new List<Type>();

        private readonly Dictionary<Type, CultureInfo> specialTypeSerializationCulture =
            new Dictionary<Type, CultureInfo>();

        private readonly Dictionary<Type, Delegate> specialTypeSerializationFunction = new Dictionary<Type, Delegate>();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyName = GetPropertyName(memberSelector);
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propertyName);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyName = GetPropertyName(memberSelector);
            excludingProperty.Add(propertyName);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludingTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0, new Stack<object>());
        }


        internal void AddTypeSerialization(Type type, Delegate function)
        {
            specialTypeSerializationFunction[type] = function;
        }

        internal void AddTypeSerializationCulture(Type type, CultureInfo culture)
        {
            specialTypeSerializationCulture[type] = culture;
        }

        internal void AddCutProperty(string propertyName, int length)
        {
            cutProperty[propertyName] = length;
        }

        private string GetPropertyName<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var memberExpression = memberSelector.Body as MemberExpression;
            var propertyName = memberExpression?.Member.Name;
            return propertyName;
        }

        private string PrintToString(object obj, int nestingLevel, Stack<object> visited)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (baseTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var objectType = obj.GetType();
            var stringBuilder = new StringBuilder(objectType.Name + Environment.NewLine);

            if (obj is IEnumerable collection) return PrintIEnumerable(collection, nestingLevel);


            foreach (var propertyInfo in objectType.GetProperties())
            {
                var propertyName = propertyInfo.Name;
                if (TryPrintValueByMemberName(obj, nestingLevel, visited, propertyInfo, stringBuilder,
                    out var innerInformation))
                    stringBuilder.Append(indentation + propertyName + " = " + innerInformation);
            }

            foreach (var fieldInfo in objectType.GetFields())
            {
                var fieldName = fieldInfo.Name;
                if (TryPrintValueByMemberName(obj, nestingLevel, visited, fieldInfo, stringBuilder,
                    out var innerInformation))
                    stringBuilder.Append(indentation + fieldName + " = " + innerInformation);
            }

            visited.Pop();
            return stringBuilder.ToString();
        }

        private bool TryPrintValueByMemberName(object obj, int nestingLevel, Stack<object> visited,
            MemberInfo memberInfo,
            StringBuilder stringBuilder,out string innerInformation)
        {
            var (memberInfoType, memberInfoValue) = GetTypeAndValueMemberInfo(memberInfo, obj);
            innerInformation = "";
            var indentation = new string('\t', nestingLevel + 1);
            
            if (excludingTypes.Contains(memberInfoType)) return false;
            if (excludingProperty.Contains(memberInfo.Name)) return false;
            
            visited.Push(obj);
            if (visited.Contains(memberInfoValue))
            {
                stringBuilder.Append(indentation)
                    .Append(memberInfo.Name)
                    .Append(" = itself")
                    .Append(Environment.NewLine);
                return false;
            }

            innerInformation = PrintToString(FormatValue(memberInfoType, memberInfoValue, memberInfo.Name),
                nestingLevel + 1, visited);
            return true;
        }

        private string PrintIEnumerable(IEnumerable collection, int nestingLvl)
        {
            var sb = new StringBuilder();
            sb.Append("[");
            foreach (var item in collection)
                sb.Append(PrintToString(item, nestingLvl, new Stack<object>()).Trim(Environment.NewLine.ToCharArray()) +
                          ", ");

            if (sb.Length > 2)
                sb.Remove(sb.Length - 2, 2);
            sb.Append("]")
                .Append(Environment.NewLine);
            return sb.ToString();
        }

        private (Type, object) GetTypeAndValueMemberInfo(MemberInfo memberInfo, object obj)
        {
            Type type;
            object value;

            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    value = ((FieldInfo) memberInfo).GetValue(obj);
                    type = ((FieldInfo) memberInfo).FieldType;
                    return (type, value);
                case MemberTypes.Property:
                    value = ((PropertyInfo) memberInfo).GetValue(obj);
                    type = ((PropertyInfo) memberInfo).PropertyType;
                    return (type, value);
                default:
                    throw new Exception("cant match memberInfo");
            }
        }

        private object FormatValue(Type type, object value, string name)
        {
            if (specialTypeSerializationFunction.TryGetValue(type, out var serializeFunction))
                return serializeFunction?.DynamicInvoke(value);

            if (specialTypeSerializationCulture.TryGetValue(type, out var serializeCultureInfo))
                return ((IFormattable) value).ToString("g", serializeCultureInfo);

            if (cutProperty.TryGetValue(name, out var maxLen))
            {
                var len = value.ToString().Length > maxLen ? maxLen : value.ToString().Length;
                return value.ToString().Substring(0, len);
            }

            return value;
        }
    }
}