using System.Collections.Generic;
using ObjectPrinting.Formatting;
using ObjectPrinting.Serializer;

namespace ObjectPrinting
{
    internal interface IPrintingConfig
    {
        void ApplyNewSerializationRule(PropertySerializationRule rule);

        IReadOnlyList<PropertySerializationRule> SerializationRules { get; }
        
        FormatConfiguration InstalledFormatting { get; }
    }
}