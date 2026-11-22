using System.Globalization;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrinterAcceptanceTests
{
    private Student student;

    [SetUp]
    public void SetUp()
    {
        student = new Student
        {
            FirstName = "Иван",
            LastName = "Петров",
            Age = 20,
            AverageGrade = 4.75,
            University = "Уральский федеральный университет",
            Specialty = "Программная инженерия",
            Course = 3,
            Scholarship = 2500.50m,
            IsBudget = true,
            EnrollmentDate = new DateTime(2024, 9, 1),
            StudentId = Guid.Parse("11111111-1111-1111-1111-111111111111")
        };
    }
    
    [Test]
    public Task Demo()
    {
        var printer = ObjectPrinter.For<Student>();

        printer
            .Exclude<int>()
            .SetTypeSerialization<string>(str => $"\"{str}\"")
            .ForProperty(s => s.AverageGrade)
                .SetNumericCulture(new CultureInfo("ru-RU"))
            .ForProperty(s => s.Age)
                .SetNumericCulture(CultureInfo.CurrentCulture)
            .ForProperty(s => s.Scholarship)
                .SetNumericCulture(new CultureInfo("en-US"))
            .ForProperty(s => s.University)
                .SetModifiedSerialization(
                name => $"Университет: \"{name}\"")
            .ForProperty(s => s.Specialty)
                .Truncate(15)
            .ForProperty(s => s.StudentId).Exclude();

            
        var serialized = printer.PrintToString(student);

        return ObjectPrinterTests.VerifySnapshot(serialized);
    }

    [Test]
    public Task DemoWithConfig()
    {
        var serializedWithConfig = student.PrintToString(config =>
        {
            config
                .Exclude<int>()
                .SetTypeSerialization<string>(str => $"\"{str}\"");

            config
                .ForProperty(s => s.AverageGrade)
                .SetNumericCulture(new CultureInfo("ru-RU"));

            config.ForProperty(s => s.Scholarship)
                .SetNumericCulture(new CultureInfo("en-US"));

            config
                .ForProperty(s => s.Age)
                .SetNumericCulture(CultureInfo.CurrentCulture);

            config
                .ForProperty(s => s.University)
                .SetModifiedSerialization(
                    name => $"Университет: \"{name}\"");

            config
                .ForProperty(s => s.Specialty)
                .Truncate(15);

            config
                .ForProperty(s => s.StudentId)
                .Exclude();
        });
        
        return ObjectPrinterTests.VerifySnapshot(serializedWithConfig);
    }

    [Test]
    public Task DemoWithEmptyConfig()
    {
        var serializedWithEmptyConfig = student.PrintToString();
        
        return ObjectPrinterTests.VerifySnapshot(serializedWithEmptyConfig);
    }
}