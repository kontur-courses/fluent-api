using System.Collections.Generic;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    public class NestedObjectContainer
    {
        public List<object> Container = new List<object>();

        public NestedObjectContainer()
        {
            List<object> currentList = Container;
            for (int i = 0; i < 30; i++)
            {
                var internalList = new List<object>();
                currentList.Add(internalList);
                currentList = internalList;
            }
        }
    }
}