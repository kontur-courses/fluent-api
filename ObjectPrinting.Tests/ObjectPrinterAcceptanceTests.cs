using System;
using System.Globalization;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test, Timeout(3_000)]
        public void Demo()
        {
            var person = PersonFactory.Get();
            var house = HouseFactory.Get();
            house.Owner = person;
            person.House = house;
            var culture = new CultureInfo("en-GB");
            var printer = ObjectPrinter.For<Person>()
                // 1. Исключение из сериализации свойства/поля определенного типа
                .Exclude<Guid>()
                .Exclude<string[]>()
                // 2. Альтернативный способ сериализации для определенного типа
                .When<int>().Use(value => $"~{value}~")
                // 3. Для всех типов, имеющих культуру, есть возможность ее указать
                .When<double>().Use(culture)
                // 4. Настройка сериализации конкретного свойства/поля
                .When(p => p.Money).Use(money => $"{money}$")
                // 5. Возможность обрезания строк
                .When<string>().UseTrimming(2)
                // 6. Исключение из сериализации конкретного свойства/поля
                .Exclude(x => x.Country)
                // 7. Корректная обработка циклических ссылок между объектами (не должны приводить к `StackOverflowException`)
                .SetAllowCycleReference(true);

            var serialized = printer.PrintToString(person);


            var expected = new StringBuilder()
                .AppendLine($"{nameof(Person)}")
                .AppendLine($"\t{nameof(Person.Name)} = {person.Name[..2]}")
                .AppendLine($"\t{nameof(Person.Height)} = {person.Height.ToString(culture)}")
                .AppendLine($"\t{nameof(Person.Age)} = ~{person.Age}~")
                .AppendLine($"\t{nameof(Person.House)} = {nameof(House)}")
                .AppendLine($"\t\t{nameof(House.Owner)} = {{...}}")
                .AppendLine($"\t\t{nameof(House.Address)} = {person.House.Address[..2]}")
                .AppendLine($"\t{nameof(Person.Money)} = {person.Money}$")
                .ToString();
            serialized.Should().Be(expected);
        }
    }
}