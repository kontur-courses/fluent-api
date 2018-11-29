using System;
using System.Collections.Generic;
using System.Globalization;


namespace ObjectPrinting
{
    public class Settings
    {
        private readonly HashSet<Type> _excludedProperties;
        private readonly Dictionary<Type, Func<Type, string>> _changedTypesSerialization;
        private readonly Dictionary<Type, CultureInfo> _changedCulturInfo;

        public Settings()
        {
            _excludedProperties = new HashSet<Type>();
            _changedTypesSerialization = new Dictionary<Type, Func<Type, string>>();
            _changedCulturInfo = new Dictionary<Type, CultureInfo>();
        }

        public void AddPropertyToExclude(Type property)
        {
            _excludedProperties.Add(property);
        }

        public HashSet<Type> GetExcludedProperties()
        {
            return _excludedProperties;
        }

        public void SetSerializationForType(Type type, Func<Type, string> newSerializationMethod)
        {
            if (_changedTypesSerialization.ContainsKey(type))
                _changedTypesSerialization[type] = newSerializationMethod;
            else
                _changedTypesSerialization.Add(type, newSerializationMethod);
        }

        public Dictionary<Type, Func<Type, string>> GetChangedTypesSerialization()
        {
            return _changedTypesSerialization;
        }

        public void SetCultureInfoForType(Type type, CultureInfo cultureInfo)
        {
            if (_changedCulturInfo.ContainsKey(type))
                _changedCulturInfo[type] = cultureInfo;
            else
                _changedCulturInfo.Add(type, cultureInfo);
        }

        public Dictionary<Type, CultureInfo> GetChangedCultureInfo()
        {
            return _changedCulturInfo;
        }
    }
}
