using System.Collections.Generic;

namespace ObjectPrinting
{
    internal interface IPrintingConfig
    {
        void ApplyNewSerializationRule(SerializationRule rule);

        IReadOnlyList<SerializationRule> SerializationRules { get; }
    }
}