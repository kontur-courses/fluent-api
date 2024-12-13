namespace ObjectPrinterTests.Tests;

public static class MockPerson
{
    public static readonly Person GetCoolProgramer = new()
    {
        Age = 20,
        Height = 19.2,
        Id = Guid.Empty,
        Name = "Андрей Захаров",
        PlaceBirth = "Uknown"
    };
}