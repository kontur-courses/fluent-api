using System.Globalization;

namespace ObjectPrinting
{
    public interface ISerializingConfig<TOwner, TPropertyType>
    {
        PrintingConfig<TOwner> SerializingConfig { get; }
    }
}