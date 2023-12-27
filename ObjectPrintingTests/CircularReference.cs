namespace ObjectPrintingTests;

public class CircularReference
{
    public CircularReference Self;

    public CircularReference() =>
        Self = this;
}