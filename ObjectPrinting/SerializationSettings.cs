using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class SerializationSettings
    {
        private readonly HashSet<Type> _typesToExclude;
        private readonly HashSet<PropertyInfo> _propertiesToExclude;
        private readonly Dictionary<Type, CultureInfo> _cultureForType;
        private readonly Dictionary<Type, Func<object, string>> _serializationForType;
        private readonly Dictionary<PropertyInfo, Func<object, string>> _serializationForProperty;
        private readonly Dictionary<PropertyInfo, int> _propertiesToTrim;
        private readonly HashSet<object> _serializedObjects;
        private static readonly HashSet<Type> _finalTypes = new HashSet<Type>()
        {
            typeof(int), typeof(uint), typeof(double), typeof(float), typeof(decimal),
            typeof(long), typeof(ulong), typeof(short), typeof(ushort),
            typeof(string), typeof(bool),
            typeof(DateTime), typeof(TimeSpan)

        };

        public SerializationSettings()
        {
            _typesToExclude = new HashSet<Type>();
            _propertiesToExclude = new HashSet<PropertyInfo>();
            _cultureForType = new Dictionary<Type, CultureInfo>();
            _serializationForType = new Dictionary<Type, Func<object, string>>();
            _serializationForProperty = new Dictionary<PropertyInfo, Func<object, string>>();
            _propertiesToTrim = new Dictionary<PropertyInfo, int>();
            _serializedObjects = new HashSet<object>();
        }

        public void AddTypeToExclude(params Type[] types)
        {
            foreach (var type in types)
                _typesToExclude.Add(type);
        }

        public void AddPropertyToExclude(params PropertyInfo[] properties)
        {
            foreach (var property in properties)
                _propertiesToExclude.Add(property);
        }

        public void AddCultureForType(Type type, CultureInfo culture)
        {
            _cultureForType.TryAdd(type, culture);
        }

        public void AddTypeSerialization<TPropType>(Func<TPropType, string> typeSerialization)
        {
            _serializationForType.TryAdd(typeof(TPropType), type => typeSerialization((TPropType)type));
        }

        public void AddPropertySerialization<TPropType>(PropertyInfo property, Func<TPropType, string> propertySerialization)
        {
            _serializationForProperty.TryAdd(property, type => propertySerialization((TPropType)type));
        }

        public void AddPropertyToTrim(PropertyInfo property, int length)
        {
            _propertiesToTrim.TryAdd(property, length);
        }

        public void AddSerializedObject(object obj)
        {
            _serializedObjects.Add(obj);
        }

        public HashSet<Type> GetExcludingTypes() => _typesToExclude;

        public HashSet<PropertyInfo> GetExcludingProperties() => _propertiesToExclude;

        public Dictionary<Type, CultureInfo> GetTypesWithCulture() => _cultureForType;

        public Dictionary<Type, Func<object, string>> GetTypesSerializations() =>
            _serializationForType;

        public Dictionary<PropertyInfo, Func<object, string>> GetPropertiesSerializations() =>
            _serializationForProperty;

        public Dictionary<PropertyInfo, int> GetPropertiesToTrim() => _propertiesToTrim;

        public HashSet<Type> GetFinalTypes() => _finalTypes;
        public HashSet<object> GetSerializedObjects() => _serializedObjects;
    }
}
