using ObjectPrinting.Interfaces;

namespace ObjectPrinting;

public class ObjectPrinter : IObjectPrinter
{
    public ObjectPrinterSettings<T> For<T>()
    {
        return new ObjectPrinterSettings<T>();
    }
}