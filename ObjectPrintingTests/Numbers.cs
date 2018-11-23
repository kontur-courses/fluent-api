using System.Collections.Generic;

namespace ObjectPrintingTests
{
    public static class Numbers
    {
        public static IEnumerable<int> GetNumbers()
        {
            var number = 0;
            while (true)
            {
                yield return number;
                number++;
            }
        }
    }
}