namespace ObjectPrinting.Contracts;

public interface ISerializer<out T>
{
    public T Serialize(object instance, int nestingLevel);
}