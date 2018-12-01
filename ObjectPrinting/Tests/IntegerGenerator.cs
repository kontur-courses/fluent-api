using System.Collections;
using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class IntegerGenerator : IEnumerable<int>
    {
        public IEnumerator<int> GetEnumerator()
        {
            var i = 0;
            while (true)
                yield return ++i;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}