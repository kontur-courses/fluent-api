using NUnit.Framework;
using System;
using System.Globalization;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrinterAcceptanceTests
{
    private Person _person = null!;

    [SetUp]
    public void SetUp()
    {
        var father = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "Aaron",
            Secondname = "Smith",
            Age = 56,
            HeightInMeters = 176.3,
            WeightInKilograms = 85.5,
            Children = []
        };

        _person = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "Alexander",
            Secondname = "Smith",
            Surname = "Junior",
            Age = 20,
            HeightInMeters = 192.54,
            WeightInKilograms = 75.3,
            Father = father
        };

        // Создаем циклическую ссылку: Отец -> Ребенок -> Отец
        father.Children.Add(_person);
    }

    [Test]
    public void Demo_ConfiguredPrinter()
    {
        var printer = ObjectPrinter.Setup<Person>()
                                        .Exclude<Guid>()
                                        .Exclude(p => p.Children)
                                        .For<double>()
                                            .SetCulture(CultureInfo.InvariantCulture)
                                            .Then()
                                        .For(p => p.HeightInMeters)
                                            .SetFormat(height => $"{height} см.")
                                            .Then()
                                        .For(p => p.FirstName)
                                            .Truncate(4)
                                            .Then()
                                        .Apply();

        var result = printer.PrintToString(_person);

        Console.WriteLine("=== Результат с полной конфигурацией ===");
        Console.WriteLine(result);
    }

    [Test]
    public void Demo_DefaultSerialization_And_CyclicReferences()
    {
        var defaultResult = _person.PrintToString();

        Console.WriteLine("=== Дефолтная сериализация с циклической ссылкой ===");
        Console.WriteLine(defaultResult);
    }

    [Test]
    public void Demo_InlineConfiguration()
    {
        var inlineResult = _person.PrintToString(config => config
            .Exclude<Guid>()
            .For(p => p.Surname)
                .SetFormat(s => s is null ? "Отсутствует" : s)
                .Then()
            .For(p => p.Father)
                // Кастомный вывод вложенного объекта: вместо раскрытия свойств отца пишем кратко
                .SetFormat(f => f is null ? "Не указан" : $"{f.FirstName} {f.Secondname} (Возраст: {f.Age})")
                .Then());

        Console.WriteLine("=== Быстрая сериализация на лету ===");
        Console.WriteLine(inlineResult);
    }
}