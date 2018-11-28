using System.Collections;
using System.Collections.Generic;

namespace ObjectPrinting.Tests.Modules
{
    internal class InfinityCollection<T> : IEnumerable<T>
    {
        public IEnumerator<T> GetEnumerator()
        {
            while (true)
                yield return default(T);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}