using System.Collections.Generic;

namespace ObjectPrinting.Extensions
{
    public class ReferenceComparer : IEqualityComparer<object>
    {
        public bool Equals(object x, object y)
        {
            return x == y;
        }

        public int GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }
    }
}