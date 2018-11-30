using System;
using System.Collections.Generic;
using System.Globalization;


namespace ObjectPrinting
{
    public class Settings
    {
        private readonly HashSet<Type> excludedTypes;
        private readonly Dictionary<Type, Func<Type, string>> changedTypesSerialization;
        private readonly Dictionary<Type, CultureInfo> changedCultureInfo;
        private readonly Dictionary<string, Func<Type, string>> changedPropertiesSerialization;
        private readonly Dictionary<string, Func<string, string>> propertiesToTrim;
        private readonly HashSet<string> excludedProperties;

        public Settings()
        {
            excludedTypes = new HashSet<Type>();
            changedTypesSerialization = new Dictionary<Type, Func<Type, string>>();
            changedCultureInfo = new Dictionary<Type, CultureInfo>();
            changedPropertiesSerialization = new Dictionary<string, Func<Type, string>>();
            propertiesToTrim = new Dictionary<string, Func<string, string>>();
            excludedProperties = new HashSet<string>();
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

        public Dictionary<Type, Func<Type, string>> GetChangedTypesSerialization()
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

        public void SetPropertySerialization(string propName, Func<Type, string> serializationFunc)
        {
            if (changedPropertiesSerialization.ContainsKey(propName))
                changedPropertiesSerialization[propName] = serializationFunc;
            else
                changedPropertiesSerialization.Add(propName, serializationFunc);
        }

        public Dictionary<string, Func<Type, string>> GetPropertiesSerialization()
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

        public void AddPropertyToExclude(string propertyName)
        {
            excludedProperties.Add(propertyName);
        }
    }
}
