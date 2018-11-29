using System;
using System.Collections.Generic;


namespace ObjectPrinting
{
    public class Settings
    {
        private readonly HashSet<Type> _excludedProperties;
        private readonly Dictionary<Type, Func<Type, string>> _changedTypesSerialization;

        public Settings()
        {
            _excludedProperties = new HashSet<Type>();
            _changedTypesSerialization = new Dictionary<Type, Func<Type, string>>();
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
    }
}
