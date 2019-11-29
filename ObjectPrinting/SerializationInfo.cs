using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ObjectPrinting
{
    public class SerializationInfo
    {
        private readonly Dictionary<Type, CultureInfo> cultureRules;
        private readonly HashSet<string> excludedProperties;
        private readonly HashSet<Type> excludedTypes;
        private readonly HashSet<Type> finalTypes;
        private readonly Dictionary<string, Func<object, string>> nameRules;
        private readonly Dictionary<string, Func<string, string>> trimRules;
        private readonly Dictionary<Type, Func<object, string>> typeRules;

        public SerializationInfo(HashSet<Type> finalTypes)
        {
            nameRules = new Dictionary<string, Func<object, string>>();
            excludedProperties = new HashSet<string>();
            excludedTypes = new HashSet<Type>();
            cultureRules = new Dictionary<Type, CultureInfo>();
            typeRules = new Dictionary<Type, Func<object, string>>();
            trimRules = new Dictionary<string, Func<string, string>>();
            this.finalTypes = finalTypes;
        }


        public void ExcludeType(Type type)
        {
            excludedTypes.Add(type);
        }

        public void ExcludeName(string excludingName)
        {
            excludedProperties.Add(excludingName);
        }

        public void AddTypeSerializationRule(Type type, Func<object, string> serialization)
        {
            typeRules[type] = serialization;
        }

        public void AddSerializationRule(string ruleName, Func<object, string> serialization)
        {
            nameRules[ruleName] = serialization;
        }

        public void AddCultureRule(Type type, CultureInfo serialization)
        {
            cultureRules[type] = serialization;
        }

        public void AddTrimRule(string trimName, int length)
        {
            trimRules[trimName] = x =>
            {
                var str = x;
                return str.Substring(0, length > str.Length ? str.Length : length);
            };
        }

        public bool Excluded(Type type, string name)
        {
            return excludedTypes.Contains(type) || excludedProperties.Contains(name);
        }

        public bool TryGetSerialization(object currentValue, Type currentType, string currentName,
            string identation, out StringBuilder stringBuilder)
        {
            stringBuilder = new StringBuilder();
            if (currentValue == null)
                return false;

            stringBuilder.Append(identation);

            if (nameRules.TryGetValue(currentName, out var propertyRule))
            {
                stringBuilder.Append(propertyRule(currentValue));
                return true;
            }

            if (typeRules.TryGetValue(currentType, out var typeRule))
            {
                stringBuilder.Append(typeRule(currentValue));
                return true;
            }

            if (trimRules.TryGetValue(currentName, out var trimRule))
            {
                stringBuilder.Append(trimRule(currentValue.ToString()));
                return true;
            }

            if (cultureRules.TryGetValue(currentType, out var cultureInfo))
            {
                stringBuilder.Append(currentName + " = ");
                stringBuilder.Append(((double) currentValue).ToString(cultureInfo));
                return true;
            }

            if (!finalTypes.Contains(currentType)
                && currentType.GetInterface(nameof(IEnumerable)) != null)
            {
                stringBuilder.Append(GetIenumerableSerialization(currentName, identation, currentValue));

                return true;
            }

            return false;
        }

        private StringBuilder GetIenumerableSerialization(string currentName, string identation,
            object currentValue)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(currentName + "=");
            stringBuilder.Append(Environment.NewLine);
            identation += '\t';
            var objIenumerable = (IEnumerable) currentValue;
            var i = 0;
            foreach (var element in objIenumerable)
            {
                stringBuilder.Append(identation + $"[{i}] = ");
                stringBuilder.Append(element);
                stringBuilder.Append(Environment.NewLine);
                i++;
            }

            return stringBuilder;
        }
    }
}