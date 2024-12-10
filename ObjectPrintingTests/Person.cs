namespace ObjectPrintingTests;

public class Person(Guid id, string name, double height, int age, DateTime birthDate)
{
    public Guid Id { get; } = id;
    public string Name { get; } = name;
    public double Height { get; } = height;
    public int Age { get; } = age;
    public decimal BestField = 22;
    public DateTime BirthDate { get; } = birthDate;
}