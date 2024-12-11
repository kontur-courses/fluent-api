using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    public class ObjectPrinterTests
    {
        private Person person;

        [SetUp]
        public void Setup()
        {
            person = new Person
                {Name = "Monkey", SecondName = "D.Luffy", NameOfPet = "Usopp", Height = 1234567.89, Age = 17};
        }

        [Test]
        public void ObjetctPrinter_ShouldCorrectPrint_WithOutExcludingType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Exclude<string>();
            string result = printer.PrintToString(person);
            result.Should().NotContain(person.Name)
                .And.NotContain(person.SecondName)
                .And.NotContain(person.NameOfPet);
        }

        [Test]
        public void ObjetctPrinter_ShouldCorrectPrint_WithOutExcludingField()
        {
            var printer = ObjectPrinter.For<Person>()
                .Exclude(p => p.SecondName);
            string result = printer.PrintToString(person);
            result.Should().NotContain(person.SecondName);
        }

        [Test]
        public void ObjetctPrinter_ShouldCorrectPrint_WithSelectedCulture()
        {
            var printer = ObjectPrinter.For<Person>()
                .SelectCulture<double>(new CultureInfo("fr-FR"));
            string result = printer.PrintToString(person);
            result.Should().Contain("1\u00a0234\u00a0567,890");
        }

        [Test]
        public void ObjetctPrinter_ShouldCorrectPrint_WithSpecialSerializeType()
        {
            var printer = ObjectPrinter.For<Person>()
                .SerializeTypeWithSpecial<double>((x) => $"~~ {x} ~~");
            string result = printer.PrintToString(person);
            result.Should().Contain("~~ 1234567,89 ~~");
        }

        [Test]
        public void ObjetctPrinter_ShouldCorrectPrint_WithSpecialSerializeField()
        {
            var printer = ObjectPrinter.For<Person>()
                .SelectField(p => p.Age).Using(x => 22.ToString());
            string result = printer.PrintToString(person);
            result.Should().Contain("22");
        }

        [Test]
        public void ObjetctPrinter_ShouldCorrectPrint_WithTrimmedStringField()
        {
            var printer = ObjectPrinter.For<Person>()
                .SelectField(p => p.SecondName).TrimmedToLength(1);
            string result = printer.PrintToString(person);
            result.Should().Contain("D").And.NotContain("D.Luffy");
        }

        [Test]
        public void ObjetctPrinter_ShouldCorrectPrint_WithExcludingPerson()
        {
            var printer = ObjectPrinter.For<Person>()
                .Exclude<Person>();
            string result = printer.PrintToString(person);
            result.Should().BeEmpty();
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
                {"Archeologist", person},
            };
            var expected = "\tDictionary`2{" + Environment.NewLine +
                           "\t\tArcheologist : Person" + Environment.NewLine +
                           "\t\t\tId = 00000000-0000-0000-0000-000000000000" + Environment.NewLine +
                           "\t\t\tName = Monkey" + Environment.NewLine +
                           "\t\t\tSecondName = D.Luffy" + Environment.NewLine +
                           "\t\t\tNameOfPet = Usopp" + Environment.NewLine +
                           "\t\t\tHeight = 1234567,89" + Environment.NewLine +
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
            List<Person> members = new List<Person> {person};
            var expected = "\tList`1{" + Environment.NewLine +
                           "\t\t[0] = Person" + Environment.NewLine +
                           "\t\t\tId = 00000000-0000-0000-0000-000000000000" + Environment.NewLine +
                           "\t\t\tName = Monkey" + Environment.NewLine +
                           "\t\t\tSecondName = D.Luffy" + Environment.NewLine +
                           "\t\t\tNameOfPet = Usopp" + Environment.NewLine +
                           "\t\t\tHeight = 1234567,89" + Environment.NewLine +
                           "\t\t\tAge = 17" + Environment.NewLine +
                           "\t\t\tCountsOfTeamMembers = null" + Environment.NewLine +
                           "\t\t\tAlliedTeams = null" + Environment.NewLine +
                           "\t\t\tTeam = null" + Environment.NewLine +
                           "\t}";
            members.PrintToString().Should().Contain(expected);
        }
    }
}