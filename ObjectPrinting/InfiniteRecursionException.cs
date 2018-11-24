using System;

namespace ObjectPrinting
{
    public class InfiniteRecursionException : Exception
    {
        public InfiniteRecursionException()
        {
        }

        public InfiniteRecursionException(string message) : base(message)
        {
        }

        public InfiniteRecursionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}