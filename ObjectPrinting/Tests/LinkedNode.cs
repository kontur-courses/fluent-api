namespace ObjectPrinting.Tests
{
    public class LinkedNode<T>
    {
        public T Value { get; set; }

        public LinkedNode<T> Next { get; set; }
    }
}