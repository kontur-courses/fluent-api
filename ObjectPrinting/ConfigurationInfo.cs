using System;
using System.Collections.Immutable;
using System.Reflection;

namespace ObjectPrinting
{
    public class ConfigurationInfo
    {
        private ImmutableHashSet<Type> typesToExclude;
        private ImmutableHashSet<string> propertiesToExclude;
        private ImmutableDictionary<Type, Delegate> usingForTypes;
        private ImmutableDictionary<string, Delegate> usingForProperties;

        public ConfigurationInfo()
        {
            typesToExclude = ImmutableHashSet<Type>.Empty;
            propertiesToExclude = ImmutableHashSet<string>.Empty;
            usingForTypes = ImmutableDictionary<Type, Delegate>.Empty;
            usingForProperties = ImmutableDictionary<string, Delegate>.Empty;
        }

        private ConfigurationInfo(ConfigurationInfo configurationInfo)
        {
            typesToExclude = configurationInfo.typesToExclude;
            propertiesToExclude = configurationInfo.propertiesToExclude;
            usingForTypes = configurationInfo.usingForTypes;
            usingForProperties = configurationInfo.usingForProperties;
        }

        public bool ShouldExclude(PropertyInfo propertyInfo) =>
            typesToExclude.Contains(propertyInfo.PropertyType) || propertiesToExclude.Contains(propertyInfo.Name);

        public ConfigurationInfo AddPropertyToExclude(string propertyName) =>
            new ConfigurationInfo(this) {propertiesToExclude = propertiesToExclude.Add(propertyName)};

        public ConfigurationInfo AddTypeToExclude(Type propertyType) =>
            new ConfigurationInfo(this) {typesToExclude = typesToExclude.Add(propertyType)};

        public ConfigurationInfo AddUsingForType<TPropType>(Func<TPropType, string> print)
        {
            var updatedUsingForTypes = usingForTypes.ContainsKey(typeof(TPropType))
                ? usingForTypes.SetItem(typeof(TPropType), print)
                : usingForTypes.Add(typeof(TPropType), print);
            return new ConfigurationInfo(this) {usingForTypes = updatedUsingForTypes};
        }

        public ConfigurationInfo AddUsingForProperty<TPropType>(Func<TPropType, string> print, string propertyName)
        {
            var updatedUsingForProperties = usingForProperties.ContainsKey(propertyName)
                ? usingForProperties.SetItem(propertyName, print)
                : usingForProperties.Add(propertyName, print);
            return new ConfigurationInfo(this) {usingForProperties = updatedUsingForProperties};
        }

        public string TryUseConfiguration(PropertyInfo propertyInfo, object obj)
        {
            usingForProperties.TryGetValue(propertyInfo.Name, out var configureAction);
            if (configureAction == null)
                usingForTypes.TryGetValue(propertyInfo.PropertyType, out configureAction);
            return (string)configureAction?.DynamicInvoke(propertyInfo.GetValue(obj));
        }
    }
}