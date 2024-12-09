using ObjectPrinterTests;

namespace ObjectPrinting.Solved.Tests;

public static class MockPerson
{
    public static readonly Person GetCoolProgramer = new Person()
    {
        Age = 20,
        Height = 19.2,
        Id = Guid.Empty,
        Name = "Андрей Захаров"
    };
}