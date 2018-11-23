using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinter_Should
    {
        [SetUp]
        public void SetUp()
        {
            newLine = Environment.NewLine;
            person = new Person {Age = 13, Height = 160, Id = Guid.Empty, Name = "Legion"};
            printer = ObjectPrinter.For<Person>();
        }

        private string newLine;
        private Person person;
        private PrintingConfig<Person> printer;

       
        [TestCase("eu-ES", "1,025", TestName = "EU culture")]
        [TestCase("us-US", "1.025", TestName = "US culture")]
        public void PrintNumbers_WithAnotherCulture(string cultureCode, string value)
        {
            var result = $"Double{newLine}\tValue = {value}{newLine}";

            ObjectPrinter.For<Double>()
                         .Printing<double>()
                         .Using(CultureInfo.CreateSpecificCulture(cultureCode))
                         .PrintToString(new Double {Value = 1.025})
                         .Should()
                         .BeEquivalentTo(result);
        }

        [TestCase(0, "", TestName = "maxLength is zero")]
        [TestCase(int.MaxValue, "Legion", TestName = "maxLength is max value")]
        [TestCase(6, "Legion", TestName = "maxLength is length of string field")]
        [TestCase(3, "Leg", TestName = "maxLength is less than length of string field and bigger than zero")]
        public void TrimStrings_When(int maxLength, string name)
        {
            var result =
                $"Person{newLine}\tId = Guid{newLine}\tName = {name}{newLine}\tHeight = 160{newLine}\tAge = 13{newLine}";
            printer.Printing<string>()
                   .TrimmedToLength(maxLength)
                   .PrintToString(person)
                   .Should()
                   .BeEquivalentTo(result);
        }

        [Test]
        public void ExcludeFields_ByGivenSelector()
        {
            var result = $"Person{newLine}\tName = Legion{newLine}\tHeight = 160{newLine}\tAge = 13{newLine}";
            printer.Excluding(p => p.Id)
                   .PrintToString(person)
                   .Should()
                   .BeEquivalentTo(result);
        }

        [Test]
        public void ExcludeFields_ByGivenType()
        {
            var result = $"Person{newLine}\tName = Legion{newLine}\tHeight = 160{newLine}\tAge = 13{newLine}";
            printer.Excluding<Guid>()
                   .PrintToString(person)
                   .Should()
                   .BeEquivalentTo(result);
        }

        [Test]
        public void OverridePrintersForSelector_WhenSeveralGiven()
        {
            var result =
                $"Person{newLine}\tId = Guid{newLine}\tName = Legion{newLine}\tHeight = 160{newLine}\tAge = 13!{newLine}";
            printer.Printing(p => p.Age)
                   .Using(i => $"{i}?")
                   .Printing(p => p.Age)
                   .Using(i => $"{i}!")
                   .PrintToString(person)
                   .Should()
                   .BeEquivalentTo(result);
        }

        [Test]
        public void OverridePrintersForType_WhenSeveralGiven()
        {
            var result =
                $"Person{newLine}\tId = Guid{newLine}\tName = Legion{newLine}\tHeight = 160{newLine}\tAge = 13!{newLine}";
            printer.Printing<int>()
                   .Using(i => $"{i}?")
                   .Printing<int>()
                   .Using(i => $"{i}!")
                   .PrintToString(person)
                   .Should()
                   .BeEquivalentTo(result);
        }

        [Test]
        public void PrintInGivenWay_ByGivenSelector()
        {
            var result =
                $"Person{newLine}\tId = Guid{newLine}\tName = Legion!{newLine}\tHeight = 160{newLine}\tAge = 13{newLine}";
            printer.Printing(p => p.Name)
                   .Using(s => $"{s}!")
                   .PrintToString(person)
                   .Should()
                   .BeEquivalentTo(result);
        }

        [Test]
        public void PrintInGivenWay_ByGivenType()
        {
            var result =
                $"Person{newLine}\tId = Guid{newLine}\tName = Legion{newLine}\tHeight = 160{newLine}\tAge = 13!{newLine}";
            printer.Printing<int>()
                   .Using(i => $"{i}!")
                   .PrintToString(person)
                   .Should()
                   .BeEquivalentTo(result);
        }

        [Test]
        public void PrintObjects_ByDefault()
        {
            var result =
                $"Person{newLine}\tId = Guid{newLine}\tName = Legion{newLine}\tHeight = 160{newLine}\tAge = 13{newLine}";
            person.PrintToString()
                  .Should()
                  .BeEquivalentTo(result);
        }

        [Test]
        public void PrintObjects_WithConfiguration()
        {
            var result = $"Person{newLine}\tName = MY NAME IS Legion{newLine}\tHeight = 160{newLine}";
            person.PrintToString(opt => opt.Excluding<Guid>()
                                           .Excluding(p => p.Age)
                                           .Printing<double>()
                                           .Using(CultureInfo.CreateSpecificCulture("us-US"))
                                           .Printing(p => p.Name)
                                           .Using(n => $"MY NAME IS {n}"))
                  .Should()
                  .BeEquivalentTo(result);
        }

        [Test]
        public void PrintPerson_WithoutConfiguration()
        {
            var result =
                $"Person{newLine}\tId = Guid{newLine}\tName = Legion{newLine}\tHeight = 160{newLine}\tAge = 13{newLine}";
            printer.PrintToString(person)
                   .Should()
                   .BeEquivalentTo(result);
        }

        [Test]
        public void Throw_WhenExcludingByInvalidSelector()
        {
            Action exclusion = () => printer.Excluding(p => p.ToString());
            exclusion.Should()
                     .Throw<ArgumentException>()
                     .WithMessage("Selector expression is invalid");
        }
    }
}
