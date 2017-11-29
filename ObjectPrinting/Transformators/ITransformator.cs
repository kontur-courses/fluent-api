namespace ObjectPrinting
{
    public interface ITransformator
    {
        string Transform(object obj);
    }

    public interface ITransformator<in T> : ITransformator
    {
        string Transform(T obj);
    }

}