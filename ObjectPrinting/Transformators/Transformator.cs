using System;

namespace ObjectPrinting
{
    public class Transformator<T> : ITransformator<T>
    {
        private readonly Func<T, string> transformFunc;
        private readonly TransformationType type;

        public TransformationType TransformationType { get; }

        public Transformator(Func<T, string> transformFunc, TransformationType type)
        {
            this.transformFunc = transformFunc;
            TransformationType = type;
        }

        public string Transform(T obj) => transformFunc(obj);

        public string Transform(object obj) => Transform((T) obj);
    }

    public static class Transformator
    {
        public static Transformator<T> CreateFrom<T>(Func<T, string> transformFunc, TransformationType type)
            => new Transformator<T>(transformFunc, type);
    }
}