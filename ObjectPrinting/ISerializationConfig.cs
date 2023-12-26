namespace ObjectPrinting
{
    public interface ISerializationConfig<TOwner, TSerialization> :
        IWrap<TOwner>,
        IUsing<TOwner, TSerialization>,
        IHasSerializationFunc
    {
    }
}