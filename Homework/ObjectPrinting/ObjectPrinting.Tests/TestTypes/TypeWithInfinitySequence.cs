using System.Collections.Generic;

namespace ObjectPrinting.Tests.TestTypes
{
    public class TypeWithInfinitySequence
    {
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