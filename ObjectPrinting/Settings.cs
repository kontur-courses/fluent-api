using System;
using System.Collections.Generic;
using System.Globalization;


namespace ObjectPrinting
{
    public class Settings
    {
        private readonly HashSet<Type> excludedTypes;
        private readonly Dictionary<Type, Delegate> changedTypesSerialization;
        private readonly Dictionary<string, Delegate> changedPropertiesSerialization;
        private readonly Dictionary<Type, CultureInfo> changedCultureInfo;
        private readonly Dictionary<string, Func<string, string>> propertiesToTrim;
        private readonly HashSet<string> excludedProperties;
        public int MaxNestingLevel { get; }

        public Settings(int maxNestingLevel = 100)
        {
            excludedTypes = new HashSet<Type>();
            changedTypesSerialization = new Dictionary<Type, Delegate>();
            changedCultureInfo = new Dictionary<Type, CultureInfo>();
            changedPropertiesSerialization = new Dictionary<string, Delegate>();
            propertiesToTrim = new Dictionary<string, Func<string, string>>();
            excludedProperties = new HashSet<string>();
            MaxNestingLevel = maxNestingLevel;
        }

        public void AddTypeToExclude(Type type)
        {
            excludedTypes.Add(type);
        }

        public HashSet<Type> GetExcludedTypes()
        {
            return excludedTypes;
        }

        public void SetSerializationForType(Type type, Func<Type, string> newSerializationMethod)
        {
            if (changedTypesSerialization.ContainsKey(type))
                changedTypesSerialization[type] = newSerializationMethod;
            else
                changedTypesSerialization.Add(type, newSerializationMethod);
        }

        public Dictionary<Type, Delegate> GetChangedTypesSerialization()
        {
            return changedTypesSerialization;
        }

        public void SetCultureInfoForType(Type type, CultureInfo cultureInfo)
        {
            if (changedCultureInfo.ContainsKey(type))
                changedCultureInfo[type] = cultureInfo;
            else
                changedCultureInfo.Add(type, cultureInfo);
        }

        public Dictionary<Type, CultureInfo> GetChangedCultureInfo()
        {
            return changedCultureInfo;
        }

        public void SetPropertySerialization(string propName, Func<Type, string> serializationFunc)
        {
            if (changedPropertiesSerialization.ContainsKey(propName))
                changedPropertiesSerialization[propName] = serializationFunc;
            else
                changedPropertiesSerialization.Add(propName, serializationFunc);
        }

        public Dictionary<string, Delegate> GetPropertiesSerialization()
        {
            return changedPropertiesSerialization;
        }

        public void SetPropertyToTrim(string propName, Func<string, string> trimFunc)
        {
            if (propertiesToTrim.ContainsKey(propName))
                propertiesToTrim[propName] = trimFunc;
            else
                propertiesToTrim.Add(propName, trimFunc);
        }

        public Dictionary<string, Func<string, string>> GetStrPropertiesToTrim()
        {
            return propertiesToTrim;
        }

        public void AddPropertyToExclude(string propertyName)
        {
            excludedProperties.Add(propertyName);
        }

        public HashSet<string> GetExcludedProperties()
        {
            return excludedProperties;
        }
    }
}
