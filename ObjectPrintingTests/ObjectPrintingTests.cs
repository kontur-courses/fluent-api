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
        public void ObjetctPrinter_ShouldCorrectPrint_WithExcludingType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Exclude<string>();

            printer.PrintToString(Person).Should().NotContain(Person.Name)
                .And.NotContain(Person.SecondName)
                .And.NotContain(Person.NameOfPet);
        }

        [Test]
        public void ObjetctPrinter_ShouldCorrectPrint_WithExcludingField()
        {
            var printer = ObjectPrinter.For<Person>()
                .Exclude(p => p.SecondName);

            printer.PrintToString(Person).Should().NotContain(Person.SecondName);
        }

        [Test]
        public void ObjetctPrinter_ShouldCorrectPrint_WithSelectedCulture()
        {
            var printerEn = ObjectPrinter.For<Person>()
                .Printing<double>().Using(new CultureInfo("en-US"));
            var printerRu = ObjectPrinter.For<Person>()
                .Printing<double>().Using(new CultureInfo("ru-Ru"));

            printerEn.PrintToString(Person).Should().Contain("168.8");
            printerRu.PrintToString(Person).Should().Contain("168,8");
        }

        [Test]
        public void ObjetctPrinter_ShouldCorrectPrint_WithSpecialSerializeType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using((x) => $"~~ {x} ~~");

            printer.PrintToString(Person).Should().Contain("~~ 168,8 ~~");
        }

        [Test]
        public void ObjetctPrinter_ShouldCorrectPrint_WithSpecialSerializeField()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Age).Using(x => 22.ToString());

            printer.PrintToString(Person).Should().Contain("22");
        }

        [Test]
        public void ObjetctPrinter_ShouldCorrectPrint_WithTrimmedStringField()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.SecondName).TrimmedToLength(1);

            printer.PrintToString(Person).Should().Contain("D").And.NotContain("D.Luffy");
        }

        [Test]
        public void ObjetctPrinter_ShouldCorrectPrint_WithExcludingPerson()
        {
            var printer = ObjectPrinter.For<Person>()
                .Exclude<Person>();

            printer.PrintToString(Person).Should().BeEmpty();
        }

        [Test]
        public void ObjetctPrinter_ShouldCorrectPrint_WithDictionary()
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
            dict.PrintToString().Should().Contain(expected);
        }

        [Test]
        public void ObjetctPrinter_ShouldCorrectPrint_WithDictionaryContainsPerson()
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
            dict.PrintToString().Should().Contain(expected);
        }

        [Test]
        public void ObjetctPrinter_ShouldCorrectPrint_WithArray()
        {
            int[] numbers = {1, 2, 3, 4, 5};
            var expected = "\tInt32[]{" + Environment.NewLine +
                           "\t\t[0] = 1" + Environment.NewLine +
                           "\t\t[1] = 2" + Environment.NewLine +
                           "\t\t[2] = 3" + Environment.NewLine +
                           "\t\t[3] = 4" + Environment.NewLine +
                           "\t\t[4] = 5" + Environment.NewLine +
                           "\t}";
            numbers.PrintToString().Should().Contain(expected);
        }

        [Test]
        public void ObjetctPrinter_ShouldCorrectPrint_WithList()
        {
            List<string> members = new List<string> {"Robin", "Usopp", "Luffy", "Zoro", "Nami"};
            var expected = "\tList`1{" + Environment.NewLine +
                           "\t\t[0] = Robin" + Environment.NewLine +
                           "\t\t[1] = Usopp" + Environment.NewLine +
                           "\t\t[2] = Luffy" + Environment.NewLine +
                           "\t\t[3] = Zoro" + Environment.NewLine +
                           "\t\t[4] = Nami" + Environment.NewLine +
                           "\t}";
            members.PrintToString().Should().Contain(expected);
        }

        [Test]
        public void ObjetctPrinter_ShouldCorrectPrint_WithLContainsPerson()
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
            members.PrintToString().Should().Contain(expected);
        }
    }
}