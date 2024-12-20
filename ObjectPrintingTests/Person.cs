namespace ObjectPrintingTests;

public record Person(Guid Id, string Name, double Height, int Age, DateTime BirthDate, decimal BestField, Person? Parent = null)
{
    public decimal BestField = BestField;
}