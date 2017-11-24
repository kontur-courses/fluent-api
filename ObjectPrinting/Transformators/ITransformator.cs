namespace ObjectPrinting
{
    public interface ITransformator
    {
        TransformationType TransformationType { get; }
        string Transform(object obj);
    }

    public interface ITransformator<in T> : ITransformator
    {
        string Transform(T obj);
    }

}