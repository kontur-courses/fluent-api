using System;
using System.Collections.Generic;
using System.Globalization;

namespace ObjectPrinting
{
    internal partial class PrintingRules
    {
        private readonly Dictionary<Type, bool> ignoredTypes = new();
        private readonly HashSet<string> ignoredProperties = new();
        
        private readonly Dictionary<Type, SerialisationRule> serialisationMethodByType = new();
        private readonly Dictionary<string, SerialisationRule> serialisationMethodByProperty = new();

        public OneTimeSetValue<CultureInfo> DefaultCulture { get; } = new(CultureInfo.CurrentCulture);
        private readonly Dictionary<Type, CultureInfo> cultures = new();

        public bool TrySetIgnore(Type type, bool allNestingLevels)
        {
            if (!allNestingLevels) return ignoredTypes.TryAdd(type, allNestingLevels);
            ignoredTypes[type] = allNestingLevels;
            return true;
        }
        
        public bool TrySetIgnore(string property)
        {
            if (ignoredProperties.Contains(property)) return false;
            ignoredProperties.Add(property);
            return true;
        }
        
        public bool TrySetSerialisation(Type type, SerialisationRule rule)
        {
            if (ignoredTypes.ContainsKey(type)) 
                throw new InvalidOperationException($"type {type.Name} already ignored");
            return serialisationMethodByType.TryAdd(type, rule);
        }
        
        public bool TrySetSerialisation(string property, SerialisationRule rule)
        {
            if (ignoredProperties.Contains(property)) 
                throw new InvalidOperationException($"property {property} already ignored");
            return serialisationMethodByProperty.TryAdd(property, rule);
        }
        
        public bool TrySetCulture(Type type, CultureInfo culture)
        {
            if (ignoredTypes.ContainsKey(type)) 
                throw new InvalidOperationException($"type {type.Name} already ignored");
            return cultures.TryAdd(type, culture);
        }
    }
}