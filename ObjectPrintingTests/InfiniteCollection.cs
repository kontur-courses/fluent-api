using System.Collections.Generic;

namespace ObjectPrintingTests;

internal static class InfiniteCollection
{
    public static IEnumerable<int> Get()
    {
        while (true)
            yield return 42;
    }
}
