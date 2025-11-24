using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrinting.Tests
{
    public class ObjectPrinterAcceptanceTests
    {

        [Test]
        public void AcceptanceTest_FluentAssertions()
        {
            var person = new Person
            {
                Id = Guid.NewGuid(),
                Name = "Alexander",
                Age = 19,
                Height = 1.85
            };

            var printerNoGuid = ObjectPrinter.For<Person>()
                .Excluding<Guid>() 
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                .Printing(p => p.Name).TrimmedToLength(5);

            string resultNoGuid = printerNoGuid.PrintToString(person);

            resultNoGuid.Should().NotBeNullOrWhiteSpace();
            resultNoGuid.Should().NotContain(person.Id.ToString());
            resultNoGuid.Should().Contain(person.Name.Substring(0, 5));
            resultNoGuid.Should().NotContain(person.Name.Substring(0, 6));
            if (person.Name.Length > 10)
                resultNoGuid.Should().NotContain(person.Name);

            var printerFormat = ObjectPrinter.For<Person>()
                .Printing<int>().Using(i => i.ToString("X"));

            string resultFormat = printerFormat.PrintToString(person);

            resultFormat.Should().Contain(person.Age.ToString("X"));

            var printerExcludeAge = ObjectPrinter.For<Person>().Excluding(p => p.Age);
            string resultExcludeAge = printerExcludeAge.PrintToString(person);
            resultExcludeAge.Should().NotContain(person.Age.ToString());
        }
    }
}
