using System.Collections.Generic;
using ObjectPrinting.Formatting;
using ObjectPrinting.Serializer;

namespace ObjectPrinting.PrintingConfigs
{
    internal interface IPrintingConfig
    {
        void ApplyNewSerializationRule(PropertySerializationRule rule);

        IReadOnlyList<PropertySerializationRule> SerializationRules { get; }
        
        FormatConfiguration InstalledFormatting { get; }
    }
}