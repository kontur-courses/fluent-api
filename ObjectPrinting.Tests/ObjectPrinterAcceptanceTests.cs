using System;
using System.Globalization;
using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test, Timeout(3_000)]
        public void Demo()
        {
            var person = PersonFactory.Get();
            var house = new House { Owner = person, Address = "New-York" };
            person.House = house;
            var culture = new CultureInfo("en-GB");
            var printer = ObjectPrinter
                .For<Person>()
                .Exclude<Guid>()
                .When<int>().Use(value => $"~{value}~")
                .When<double>().Use(culture)
                .When(p => p.Money).Use(money => $"{money}$")
                .When<string>().UseSubstring(..2)
                .Exclude(x => x.Country)
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