using System.Collections.Immutable;

namespace ObjectPrinting.Interfaces;

public interface ISerializer
{
    public string PrintToString(object obj, ImmutableList<object> previous);
}