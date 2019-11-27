using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        private readonly Person person = new Person { Name = "James", Age = 30, Height = 182.2 };

        [Test]
        public void ExcludingSpecificTypeShouldWorkCorrect()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<int>();

            printer.PrintToString(person).Should().NotContain("30").And.Contain("James");
        }

        [Test]
        public void ExcludingSpecificPropertyShouldWorkCorrect()
        {
            var printer = ObjectPrinter.For<Person>().Excluding(p => p.Name);

            printer.PrintToString(person).Should().NotContain("James").And.NotContain("Name");
        }

        [Test]
        public void UseWithCultureShouldWorkCorrect()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(CultureInfo.GetCultureInfo("en-GB"));

            printer.PrintToString(person).Should().NotContain("182,2").And.Contain("182.2");
            printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(CultureInfo.GetCultureInfo("de"));

            printer.PrintToString(person).Should().NotContain("182.2").And.Contain("182,2");
        }

        [Test]
        public void UseWithSpecificSerializationShouldWorkCorrect()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(i => i.ToString("X"));

            printer.PrintToString(person).Should().NotContain("30").And.Contain("1E");
        }

        [Test]
        public void TrimmedToLengthShouldWorkCorrect()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(4);

            printer.PrintToString(person).Should().Contain("Jame").And.NotContain("James");
        }

        [Test]
        public void PrintToStringShouldWorkCorrectWithConfig()
        {
            person.PrintToString(s => s.Excluding(p => p.Name))
                .Should().NotContain("James");
        }

        [Test]
        public void PrintToStringShouldWorkCorrectWithList()
        {
            var person1 = new Person { Name = "Alex", Age = 19, Height = 170.2 };
            var listOfPersons = new List<Person> { person, person1 };
            var printer = ObjectPrinter.For<List<Person>>();
            printer.PrintToString(listOfPersons).Should().Contain("0 : Person").And.Contain("1 : Person");
        }

        [Test]
        public void PrintToStringShouldWorkCorrectWithDictionary()
        {
            var person1 = new Person { Name = "Alex", Age = 19, Height = 170.2 };
            var dictOfPersons = new Dictionary<string, Person> { { "olderPerson", person}, {"youngerPerson", person1 } };
            var printer = ObjectPrinter.For<Dictionary<string, Person>>();
            printer.PrintToString(dictOfPersons).Should().Contain("olderPerson : Person").And.Contain("youngerPerson : Person");
        }


    }
}
