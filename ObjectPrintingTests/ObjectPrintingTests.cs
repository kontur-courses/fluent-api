using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Tests;

namespace ObjectPrintingTests;

public class ObjectPrintingTests
{
    public class ObjectPrinterTests
    {
        private static readonly Person Person = new()
            {Name = "Monkey", SecondName = "D.Luffy", NameOfPet = "Usopp", Height = 168.8, Age = 17};

        [Test]
        public void ObjectPrinter_ShouldCorrectPrint_WithExcludingType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Exclude<string>();

            var actual = printer.PrintToString(Person);

            actual.Should().NotContain(Person.Name)
                .And.NotContain(Person.SecondName)
                .And.NotContain(Person.NameOfPet);
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectPrint_WithExcludingField()
        {
            var printer = ObjectPrinter.For<Person>()
                .Exclude(p => p.SecondName);

            var actual = printer.PrintToString(Person);

            actual.Should().NotContain(Person.SecondName);
        }


        [TestCase("en-US", "168.8")]
        [TestCase("ru-RU", "168,8")]
        public void ObjectPrinter_ShouldCorrectPrint_WithSelectedCulture(string culture, string expectedValue)
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(new CultureInfo(culture));

            var actual = printer.PrintToString(Person);

            actual.Should().Contain(expectedValue);
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectPrint_WithSpecialSerializeType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using((x) => $"~~ {x} ~~");

            var actual = printer.PrintToString(Person);

            actual.Should().Contain("~~ 168,8 ~~");
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectPrint_WithSpecialSerializeField()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Age).Using(x => 22.ToString());

            var actual = printer.PrintToString(Person);

            actual.Should().Contain("22");
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectPrint_WithTrimmedStringField()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.SecondName).TrimmedToLength(1);

            var actual = printer.PrintToString(Person);

            actual.Should().Contain("D").And.NotContain("D.Luffy");
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectPrint_WithExcludingPerson()
        {
            var printer = ObjectPrinter.For<Person>()
                .Exclude<Person>();

            var actual = printer.PrintToString(Person);

            actual.Should().BeEmpty();
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectPrint_WithDictionary()
        {
            var dict = new Dictionary<string, string>
            {
                {"Robin", "Archeologist"},
                {"Usopp", "Cannoneer"},
                {"Luffy", "Captain"}
            };
            var expected = "\tDictionary`2{" + Environment.NewLine +
                           "\t\tRobin : Archeologist" + Environment.NewLine +
                           "\t\tUsopp : Cannoneer" + Environment.NewLine +
                           "\t\tLuffy : Captain" + Environment.NewLine +
                           "\t}";

            var actual = dict.PrintToString();

            actual.Should().Contain(expected);
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectPrint_WithDictionaryContainsPerson()
        {
            var dict = new Dictionary<string, Person>
            {
                {"Archeologist", Person},
            };
            var expected = "\tDictionary`2{" + Environment.NewLine +
                           "\t\tArcheologist : Person" + Environment.NewLine +
                           "\t\t\tId = 00000000-0000-0000-0000-000000000000" + Environment.NewLine +
                           "\t\t\tName = Monkey" + Environment.NewLine +
                           "\t\t\tSecondName = D.Luffy" + Environment.NewLine +
                           "\t\t\tNameOfPet = Usopp" + Environment.NewLine +
                           "\t\t\tHeight = 168,8" + Environment.NewLine +
                           "\t\t\tAge = 17" + Environment.NewLine +
                           "\t\t\tCountsOfTeamMembers = null" + Environment.NewLine +
                           "\t\t\tAlliedTeams = null" + Environment.NewLine +
                           "\t\t\tTeam = null" + Environment.NewLine +
                           "\t}";

            var actual = dict.PrintToString();

            actual.Should().Contain(expected);
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectPrint_WithArray()
        {
            int[] numbers = {1, 2, 3, 4, 5};
            var expected = "\tInt32[]{" + Environment.NewLine +
                           "\t\t[0] = 1" + Environment.NewLine +
                           "\t\t[1] = 2" + Environment.NewLine +
                           "\t\t[2] = 3" + Environment.NewLine +
                           "\t\t[3] = 4" + Environment.NewLine +
                           "\t\t[4] = 5" + Environment.NewLine +
                           "\t}";

            var actual = numbers.PrintToString();

            actual.Should().Contain(expected);
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectPrint_WithList()
        {
            List<string> members = new List<string> {"Robin", "Usopp", "Luffy", "Zoro", "Nami"};
            var expected = "\tList`1{" + Environment.NewLine +
                           "\t\t[0] = Robin" + Environment.NewLine +
                           "\t\t[1] = Usopp" + Environment.NewLine +
                           "\t\t[2] = Luffy" + Environment.NewLine +
                           "\t\t[3] = Zoro" + Environment.NewLine +
                           "\t\t[4] = Nami" + Environment.NewLine +
                           "\t}";

            var actual = members.PrintToString();

            actual.Should().Contain(expected);
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectPrint_WithLContainsPerson()
        {
            List<Person> members = new List<Person> {Person};
            var expected = "\tList`1{" + Environment.NewLine +
                           "\t\t[0] = Person" + Environment.NewLine +
                           "\t\t\tId = 00000000-0000-0000-0000-000000000000" + Environment.NewLine +
                           "\t\t\tName = Monkey" + Environment.NewLine +
                           "\t\t\tSecondName = D.Luffy" + Environment.NewLine +
                           "\t\t\tNameOfPet = Usopp" + Environment.NewLine +
                           "\t\t\tHeight = 168,8" + Environment.NewLine +
                           "\t\t\tAge = 17" + Environment.NewLine +
                           "\t\t\tCountsOfTeamMembers = null" + Environment.NewLine +
                           "\t\t\tAlliedTeams = null" + Environment.NewLine +
                           "\t\t\tTeam = null" + Environment.NewLine +
                           "\t}";

            var actual = members.PrintToString();

            actual.Should().Contain(expected);
        }
    }
}