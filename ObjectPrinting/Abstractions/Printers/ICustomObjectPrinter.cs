namespace ObjectPrinting.Abstractions.Printers;

public interface ICustomObjectPrinter
{
    string PrintToString(object obj);
}