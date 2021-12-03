using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting.Tests
{
    class MyCollection<T> : ICollection<T>, ICollection
    {
        public int Count => 42;

        private readonly List<T> list = new List<T>();

        public bool IsReadOnly => true;

        public bool IsSynchronized => false;

        public object SyncRoot => null;

        public void Add(T item)
        {
            list.Add(item);
        }

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(T item)
        {
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public bool Remove(T item)
        {
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)list).CopyTo(array, index);
        }
    }
}
