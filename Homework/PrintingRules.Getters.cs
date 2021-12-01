using System;
using System.Globalization;

namespace Homework
{
    internal partial class PrintingRules
    {
        public bool ShouldIgnore(Type type) => ignoredTypes.ContainsKey(type);
        public bool ShouldIgnore(string property) => ignoredProperties.Contains(property);
        
        public bool ShouldIgnoreInAllNestingLevel(Type type) => ignoredTypes.TryGetValue(type, out var nesting) && nesting;

        public CultureInfo GetCultureForType(Type type) 
            => cultures.TryGetValue(type, out var cultureInfo) 
                ? cultureInfo 
                : DefaultCulture.Value;
        
        public bool TryGetSerialisationMethodForType(Type type, out SerialisationRule? serialisationRule)
        {
            return serialisationMethodByType.TryGetValue(type, out serialisationRule);
        }
        
        public bool TryGetSerialisationMethodForProperty(string property, out SerialisationRule? serialisationRule)
        {
            return serialisationMethodByProperty.TryGetValue(property, out serialisationRule);
        }
    }
}