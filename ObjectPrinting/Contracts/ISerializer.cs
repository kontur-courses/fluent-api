namespace ObjectPrinting.Contracts;

public interface ISerializer
{
    public string Serialize(object instance, int nestingLevel);
}