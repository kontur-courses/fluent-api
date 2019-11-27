using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ObjectPrinting
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        private Person person;
        private Band band;

        [SetUp]
        public void BaseSetUp()
        {
            person = new Person
            {
                Name = "John",
                Age = 20,
                Height = 179.2,
                Id = Guid.Empty
            };

            band = new Band
            {
                Name = "The Beatles",
                MembersNames = new List<string>()
                {
                    "John",
                    "Ringo",
                    "Paul",
                    "George"
                },
                Instruments = new Dictionary<string, string>()
                {
                    {"George", "Sitar" }
                }
            };
        }

        [Test]
        public void Should_WorkCorrectly_OnNull()
        {
            string name = null;
            name.PrintToString().Should().BeEquivalentTo("null" + Environment.NewLine);
        }

        [Test]
        public void Should_ExcludeByPropertyCorrectly()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Id);
            var printing = printer.PrintToString(person);
            var expected = "Person" + Environment.NewLine +
                $"\tName = {person.Name}" + Environment.NewLine +
                $"\tHeight = {person.Height}" + Environment.NewLine +
                $"\tAge = {person.Age}" + Environment.NewLine;
            printing.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Should_ExcludeByTypeCorrectly()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<int>();
            var printing = printer.PrintToString(person);
            var expected = "Person" + Environment.NewLine +
                $"\tId = {person.Id}" + Environment.NewLine +
                $"\tName = {person.Name}" + Environment.NewLine +
                $"\tHeight = {person.Height}" + Environment.NewLine;
            printing.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Should_SerializeByTypeCorrectly()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<int>()
                    .Using(i => "40");
            var printing = printer.PrintToString(person);
            var expected = "Person" + Environment.NewLine +
                $"\tId = {person.Id}" + Environment.NewLine +
                $"\tName = {person.Name}" + Environment.NewLine +
                $"\tHeight = {person.Height}" + Environment.NewLine +
                "\tAge = 40" + Environment.NewLine;
            printing.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Should_SerializeByPropertyCorrectly()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name)
                    .Using(name => name.ToLower());
            var printing = printer.PrintToString(person);
            var expected = "Person" + Environment.NewLine +
                $"\tId = {person.Id}" + Environment.NewLine +
                $"\tName = {person.Name.ToLower()}" + Environment.NewLine +
                $"\tHeight = {person.Height}" + Environment.NewLine +
                $"\tAge = {person.Age}" + Environment.NewLine;
            printing.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Should_TrimStringsCorrectly()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name)
                    .TrimmedToLength(2);
            var printing = printer.PrintToString(person);
            var expected = "Person" + Environment.NewLine +
                $"\tId = {person.Id}" + Environment.NewLine +
                $"\tName = Jo" + Environment.NewLine +
                $"\tHeight = {person.Height}" + Environment.NewLine +
                $"\tAge = {person.Age}" + Environment.NewLine;
            printing.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Should_ChangeCultureInfoCorrectly()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>()
                .Using(CultureInfo.InstalledUICulture);
            var printing = printer.PrintToString(person);
            var expected = "Person" + Environment.NewLine +
                $"\tId = {person.Id}" + Environment.NewLine +
                $"\tName = {person.Name}" + Environment.NewLine +
                $"\tHeight = {person.Height.ToString(CultureInfo.InstalledUICulture)}" +
                Environment.NewLine +
                $"\tAge = {person.Age}" + Environment.NewLine;
            printing.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Should_WorkCorrectly_OnLists()
        {
            var expected = "Band" + Environment.NewLine +
                $"\tName = {band.Name}" + Environment.NewLine +
                "\tMembersNames = [" + Environment.NewLine +
                "\t\tJohn" + Environment.NewLine +
                "\t\tRingo" + Environment.NewLine +
                "\t\tPaul" + Environment.NewLine +
                "\t\tGeorge" + Environment.NewLine +
                "\t]" + Environment.NewLine;
            band.PrintToString(s => s.Excluding(b => b.Instruments))
                .Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Should_WorkCorrectly_OnDictionaries()
        {
            var expected = "Band" + Environment.NewLine +
                $"\tName = {band.Name}" + Environment.NewLine +
                "\tInstruments = [" + Environment.NewLine +
                "\t\t{" + Environment.NewLine +
                "\t\tKey:" + Environment.NewLine +
                "\t\t\tGeorge" + Environment.NewLine +
                "\t\tValue:" + Environment.NewLine +
                "\t\t\tSitar" + Environment.NewLine +
                "\t\t}" + Environment.NewLine +
                "\t]" + Environment.NewLine;
            band.PrintToString(s => s.Excluding(b => b.MembersNames))
                .Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Should_NotThrowStackOverflowException_OnSelfReflictingObject()
        {
            var mirror = new Mirror();
            mirror.Reflection = mirror;
            Action act = () => mirror.PrintToString();
            act.Should().NotThrow<StackOverflowException>();
        }
    }
}