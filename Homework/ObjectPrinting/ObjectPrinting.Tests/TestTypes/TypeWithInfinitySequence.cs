using System.Collections.Generic;

namespace ObjectPrinting.Tests.TestTypes
{
    public class TypeWithInfinitySequence
    {
        // InfinitySequence used implicitly when calling PrintToString() for instance in PrintToStringTests class.
        // ReSharper disable once UnusedMember.Global
        public IEnumerable<int> InfinitySequence
        {
            get
            {
                while (true)
                    yield return 0;
                // ReSharper disable once IteratorNeverReturns
            }
        }
    }
}