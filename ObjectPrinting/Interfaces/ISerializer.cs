using System.Collections.Immutable;

namespace ObjectPrinting.Interfaces;

public interface ISerializer<in T>
{
    public string PrintToString(object obj, ImmutableList<object> previous);
    public string PrintToString(T obj);
}