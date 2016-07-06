namespace FluentMapping
{
    public interface IMapper<TTgt, TSrc>
    {
        TTgt Map(TSrc source);
    }
}