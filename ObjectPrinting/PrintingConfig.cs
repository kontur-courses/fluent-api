using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> finalTypes = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };
        private readonly HashSet<Type> excludedTypes = new HashSet<Type>();
        private readonly HashSet<string> excludedFieldsProperties = new HashSet<string>();
        private readonly Dictionary<Type, Delegate> specialSerializationsForTypes =
            new Dictionary<Type, Delegate>();
        private readonly Dictionary<string, Delegate> specialSerializationsForFieldsProperties =
            new Dictionary<string, Delegate>();
        private CultureInfo culture = CultureInfo.InvariantCulture;
        private int resultStartIndex;
        private int resultLength = int.MaxValue;


        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            var sb = new StringBuilder();
            if (nestingLevel >= 100)
                return GetTrimString(sb);

            if (obj == null)
                return "null" + Environment.NewLine;


            var objType = obj.GetType();

            if (finalTypes.Contains(objType) && !excludedTypes.Contains(objType))
            {
                if (obj is IFormattable formattable)
                    return formattable.ToString(null, culture) + Environment.NewLine;
                return obj + Environment.NewLine;
            }

            var identation = new string('\t', nestingLevel + 1);
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance).Cast<MemberInfo>();
            var props = type.GetProperties().Cast<MemberInfo>();
            var fieldsAndProperties = fields.Union(props);

            foreach (var memberInfo in fieldsAndProperties)
            {
                string memberName;
                Type memberType;
                object memberValue;
                if (memberInfo is PropertyInfo propertyInfo)
                {
                    memberName = propertyInfo.Name;
                    memberType = propertyInfo.PropertyType;
                    memberValue = propertyInfo.GetValue(obj);
                }
                else if (memberInfo is FieldInfo fieldInfo)
                {
                    memberName = fieldInfo.Name;
                    memberType = fieldInfo.FieldType;
                    memberValue = fieldInfo.GetValue(obj);
                }
                else
                    throw new ArgumentException();

                if (excludedTypes.Contains(memberType)
                    || excludedFieldsProperties.Contains(memberName))
                    continue;

                Delegate serializeDelegate = null;

                if (specialSerializationsForFieldsProperties.ContainsKey(memberName))
                    serializeDelegate = specialSerializationsForFieldsProperties[memberName];

                else if (specialSerializationsForTypes.ContainsKey(memberType))
                    serializeDelegate = specialSerializationsForTypes[memberType];

                if (serializeDelegate != null)
                {
                    sb.Append(identation + memberName + " = " +
                              PrintToString(serializeDelegate.DynamicInvoke(memberValue),
                                  nestingLevel + 1));
                }
                else
                {
                    sb.Append(identation + memberName + " = " +
                              PrintToString(memberValue,
                                  nestingLevel + 1));
                }
            }
            return (nestingLevel != 0) ? sb.ToString() : GetTrimString(sb);
        }

        private string GetTrimString(StringBuilder resultString)
        {
            var a = resultString.ToString()
                .Substring(resultStartIndex, Math.Min(resultLength, resultString.Length)-resultStartIndex);
            return a;
        }

    public PrintingConfig<TOwner> ExcludedType<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> SpecialSerializationType<T>(Func<T, string> serialization)
        {
            specialSerializationsForTypes[typeof(T)] = serialization;
            return this;
        }

        public PrintingConfig<TOwner> SpecialSerializationField<T>(string fieldName, Func<T, string> serialization)
        {
            specialSerializationsForFieldsProperties[fieldName] = serialization;
            return this;
        }

        public PrintingConfig<TOwner> ExcludedField(string name)
        {
            excludedFieldsProperties.Add(name);
            return this;
        }

        public PrintingConfig<TOwner> SetCulture(CultureInfo inputCulture)
        {
            culture = inputCulture;
            return this;
        }

        public PrintingConfig<TOwner> Trim(int start = 0, int length = int.MaxValue) //nuint ?
        {
            resultStartIndex = start;
            resultLength = length;
            return this;
        }
    }
}