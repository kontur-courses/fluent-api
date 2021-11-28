using System.Text;

namespace ObjectPrinting.Serializers
{
    public interface ISerializer
    {
        bool CanSerialize(object obj);
        StringBuilder Serialize(object obj);
        StringBuilder Serialize(object obj, Nesting nesting);
    }
}