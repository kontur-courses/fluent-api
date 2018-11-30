using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class Sequences
    {
        public  IEnumerable<int> Fibonacci
        {
            get
            {
                var previous = 1;
                var current = 1;
                yield return 1;
                yield return 1;
                while (true)
                {
                    var next = previous + current;
                    previous = current;
                    current = next;
                    yield return next;
                }
            }
        }
    }
}