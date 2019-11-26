using System.Collections.Generic;
using ObjectPrinting.Formatting;

namespace ObjectPrinting.Serializer
{
    public class SerializationConfiguration
    {
        public readonly IReadOnlyList<PropertySerializationRule> SerializationRules;
        public readonly FormatConfiguration Formatting;
        public readonly HashSet<object> PrintedObjects;
        
        public SerializationConfiguration(IReadOnlyList<PropertySerializationRule> serializationRules, FormatConfiguration formatting)
        {
            SerializationRules = serializationRules;
            Formatting = formatting;
            PrintedObjects = new HashSet<object>();
        }

        public bool CanPrintIfRecursion(object obj, int nestingLevel)
        {
            return PrintedObjects.Contains(obj) || nestingLevel >= Formatting.MaximumRecursionDepth;
        }
    }
}