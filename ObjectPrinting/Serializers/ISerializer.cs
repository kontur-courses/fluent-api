using System.Text;

namespace ObjectPrinting.Serializers
{
    public interface ISerializer
    {
        StringBuilder Serialize(object obj, Nesting nesting);
        StringBuilder Serialize(object obj);
        bool CanSerialize(object obj);
    }
}