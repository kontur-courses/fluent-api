using System;

namespace ObjectPrinting
{
    public class Transformator<T> : ITransformator<T>
    {
        private readonly Func<T, string> transformFunc;

        public Transformator(Func<T, string> transformFunc)
        {
            this.transformFunc = transformFunc;
        }

        public string Transform(T obj) => transformFunc(obj);

        public string Transform(object obj) => Transform((T) obj);
    }

    public static class Transformator
    {
        public static Transformator<T> CreateFrom<T>(Func<T, string> transformFunc) => new Transformator<T>(transformFunc);
    }
}