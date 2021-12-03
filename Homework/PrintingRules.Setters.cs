using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Homework
{
    internal partial class PrintingRules : ICloneable
    {
        private Dictionary<Type, bool> ignoredTypes = new();
        private HashSet<string> ignoredProperties = new();
        
        private Dictionary<Type, SerialisationRule> serialisationMethodByType = new();
        private Dictionary<string, SerialisationRule> serialisationMethodByProperty = new();

        public OneTimeSetValue<CultureInfo> DefaultCulture { get; private init; } = new(CultureInfo.CurrentCulture);
        private Dictionary<Type, CultureInfo> cultures = new();

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

        public void CheckForInvalidOperations(string? name = null, Type? type = null)
        {
            if (name is not null && ignoredProperties.Contains(name)
                || type is not null && ignoredTypes.ContainsKey(type))
                throw new InvalidOperationException($"{type} {name} already ignored");
            
            if (type is not null && cultures.ContainsKey(type))
                throw new InvalidOperationException($"culture for {type} {name} already setted");
            
            if (name is not null && serialisationMethodByProperty.ContainsKey(name)
                || type is not null && (serialisationMethodByType.ContainsKey(type) 
                                        || serialisationMethodByProperty.Any(x => x.Value.Type == type)))
                throw new InvalidOperationException($"printing for {type} {name} already setted");
        }
        
        public object Clone()
        {
            return new PrintingRules
            {
                ignoredTypes = new Dictionary<Type, bool>(ignoredTypes),
                serialisationMethodByType = new Dictionary<Type, SerialisationRule>(serialisationMethodByType),
                serialisationMethodByProperty = new Dictionary<string, SerialisationRule>(serialisationMethodByProperty),
                ignoredProperties = new HashSet<string>(ignoredProperties),
                cultures = new Dictionary<Type, CultureInfo>(cultures),
                DefaultCulture = (OneTimeSetValue<CultureInfo>)DefaultCulture.Clone()
            };
        }
    }
}