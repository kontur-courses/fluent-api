namespace ObjectPrinting.Serialization;

internal interface ISerializationStrategy
{
    bool CanSerialize(object obj);
    string Serialize(object obj, int nestingLevel, ObjectSerializer serializer);
}
