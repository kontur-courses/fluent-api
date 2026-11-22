using System.Globalization;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrinterAcceptanceTests
{
    [Test]
    public Task Demo()
    {
        var student = new Student
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

        var printer = ObjectPrinter.For<Student>();

        // 1. Исключить из сериализации свойства определенного типа
        // 2. Указать альтернативный способ сериализации для определенного типа
        // 3. Настроить культуру для числового свойства
        // 4. Настроить сериализацию конкретного свойства
        // 5. Обрезать строковое свойство
        // 6. Исключить конкретное свойство

        printer
            .Exclude<int>()
            .SetTypeSerialization<string>(str => $"\"{str}\"")
            .ForProperty(s => s.AverageGrade)
                .SetNumericCulture(new CultureInfo("ru-RU"))
            .ForProperty(s => s.Age)
                .SetNumericCulture(CultureInfo.CurrentCulture)
            .SetModifiedSerialization(
                s => s.University,
                name => $"Университет: \"{name}\"")
            .Truncate(s => s.Specialty, 15)
            .Exclude(s => s.StudentId);

            
        var serialized = printer.PrintToString(student);

        //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
        //8. ...с конфигурированием

        return ObjectPrinterTests.VerifySnapshot(serialized);
    }
}