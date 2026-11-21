namespace ObjectPrinting.Tests.Models;

public class ObjectWithFields(int publicField, double privateField)
{
    public int PublicField = publicField;

    private double _privateField = privateField;
}