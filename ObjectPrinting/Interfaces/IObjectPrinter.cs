namespace ObjectPrinting.Interfaces;

public interface IObjectPrinter
{
    ObjectPrinterSettings<T> For<T>();
}