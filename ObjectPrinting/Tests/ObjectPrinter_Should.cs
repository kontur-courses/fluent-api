using System;
using System.Collections.Generic;
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
            _newLine = Environment.NewLine;
            _person = new Person {Age = 13, Height = 160, Id = Guid.Empty, Name = "Legion"};
            _printer = ObjectPrinter.For<Person>();
        }

        private string _newLine;

        private Person _person;

        private PrintingConfig<Person> _printer;

        [TestCase("eu-ES", "1,025", TestName = "EU culture")]
        [TestCase("us-US", "1.025", TestName = "US culture")]
        public void PrintNumbers_WithAnotherCulture(string cultureCode, string value)
        {
            var result = $"TestingDouble{_newLine}\tValue = {value}{_newLine}".ToHashSet(_newLine);

            ObjectPrinter.For<TestingDouble>()
                         .Printing<double>()
                         .Using(CultureInfo.CreateSpecificCulture(cultureCode))
                         .PrintToString(new TestingDouble {Value = 1.025})
                         .ToHashSet(_newLine)
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
                $"Person{_newLine}\tId = {Guid.Empty}{_newLine}\tName = {name}{_newLine}\tHeight = 160{_newLine}\tAge = 13{_newLine}"
                    .ToHashSet(_newLine);
            _printer.Printing<string>()
                    .TrimmedToLength(maxLength)
                    .PrintToString(_person)
                    .ToHashSet(_newLine)
                    .Should()
                    .BeEquivalentTo(result);
        }

        [Test]
        public void ExcludeFields_ByGivenSelector()
        {
            var result = $"Person{_newLine}\tName = Legion{_newLine}\tHeight = 160{_newLine}\tAge = 13{_newLine}"
                .ToHashSet(_newLine);
            _printer.Excluding(p => p.Id)
                    .PrintToString(_person)
                    .ToHashSet(_newLine)
                    .Should()
                    .BeEquivalentTo(result);
        }

        [Test]
        public void ExcludeFields_ByGivenType()
        {
            var result = $"Person{_newLine}\tName = Legion{_newLine}\tHeight = 160{_newLine}\tAge = 13{_newLine}"
                .ToHashSet(_newLine);
            _printer.Excluding<Guid>()
                    .PrintToString(_person)
                    .ToHashSet(_newLine)
                    .Should()
                    .BeEquivalentTo(result);
        }

        [Test]
        public void OverridePrintersForSelector_WhenSeveralGiven()
        {
            var result =
                $"Person{_newLine}\tId = {Guid.Empty}{_newLine}\tName = Legion{_newLine}\tHeight = 160{_newLine}\tAge = 13!{_newLine}"
                    .ToHashSet(_newLine);
            _printer.Printing(p => p.Age)
                    .Using(i => $"{i}?")
                    .Printing(p => p.Age)
                    .Using(i => $"{i}!")
                    .PrintToString(_person)
                    .ToHashSet(_newLine)
                    .Should()
                    .BeEquivalentTo(result);
        }

        [Test]
        public void OverridePrintersForType_WhenSeveralGiven()
        {
            var result =
                $"Person{_newLine}\tId = {Guid.Empty}{_newLine}\tName = Legion{_newLine}\tHeight = 160{_newLine}\tAge = 13!{_newLine}"
                    .ToHashSet(_newLine);
            _printer.Printing<int>()
                    .Using(i => $"{i}?")
                    .Printing<int>()
                    .Using(i => $"{i}!")
                    .PrintToString(_person)
                    .ToHashSet(_newLine)
                    .Should()
                    .BeEquivalentTo(result);
        }

        [Test]
        public void PrintFields()
        {
            ObjectPrinter.For<ThereIs>()
                         .PrintToString(new ThereIs())
                         .Should()
                         .BeEquivalentTo($"ThereIs{_newLine}\tACow = at the forest{_newLine}");
        }

        [Test]
        public void PrintInfiniteSequence_InFiniteWay()
        {
            ObjectPrinter.For<IntegerGenerator>()
                         .WithSequenceAmountRestriction(2)
                         .PrintToString(new IntegerGenerator())
                         .Should()
                         .BeEquivalentTo($"IntegerGenerator{_newLine}Containing:{_newLine}\t\t1{_newLine}\t\t2{_newLine}");
        }

        [Test]
        public void PrintInGivenWay_ByGivenSelector()
        {
            var result =
                $"Person{_newLine}\tId = {Guid.Empty}{_newLine}\tName = Legion!{_newLine}\tHeight = 160{_newLine}\tAge = 13{_newLine}"
                    .ToHashSet(_newLine);
            _printer.Printing(p => p.Name)
                    .Using(s => $"{s}!")
                    .PrintToString(_person)
                    .ToHashSet(_newLine)
                    .Should()
                    .BeEquivalentTo(result);
        }

        [Test]
        public void PrintInGivenWay_ByGivenType()
        {
            var result =
                $"Person{_newLine}\tId = {Guid.Empty}{_newLine}\tName = Legion{_newLine}\tHeight = 160{_newLine}\tAge = 13!{_newLine}"
                    .ToHashSet(_newLine);
            _printer.Printing<int>()
                    .Using(i => $"{i}!")
                    .PrintToString(_person)
                    .ToHashSet(_newLine)
                    .Should()
                    .BeEquivalentTo(result);
        }

        [Test]
        public void PrintObjects_ByDefault()
        {
            var result =
                $"Person{_newLine}\tId = {Guid.Empty}{_newLine}\tName = Legion{_newLine}\tHeight = 160{_newLine}\tAge = 13{_newLine}"
                    .ToHashSet(_newLine);
            _person.PrintToString()
                   .ToHashSet(_newLine)
                   .Should()
                   .BeEquivalentTo(result);
        }

        [Test]
        public void PrintObjects_WithConfiguration()
        {
            var result =
                $"Person{_newLine}\tName = MY NAME IS Legion{_newLine}\tHeight = 160{_newLine}".ToHashSet(_newLine);
            _person.PrintToString(opt => opt.Excluding<Guid>()
                                            .Excluding(p => p.Age)
                                            .Printing<double>()
                                            .Using(CultureInfo.CreateSpecificCulture("us-US"))
                                            .Printing(p => p.Name)
                                            .Using(n => $"MY NAME IS {n}"))
                   .ToHashSet(_newLine)
                   .Should()
                   .BeEquivalentTo(result);
        }

        [Test]
        public void PrintPerson_WithoutConfiguration()
        {
            var result =
                $"Person{_newLine}\tId = {Guid.Empty}{_newLine}\tName = Legion{_newLine}\tHeight = 160{_newLine}\tAge = 13{_newLine}"
                    .ToHashSet(_newLine);
            _printer.PrintToString(_person)
                    .ToHashSet(_newLine)
                    .Should()
                    .BeEquivalentTo(result);
        }

        [Test]
        public void PrintSequences()
        {
            var result =
                $"HashSet`1{_newLine}\tCount = 3{_newLine}\tComparer = GenericEqualityComparer`1{_newLine}Containing:{_newLine}\t\t1{_newLine}\t\t2{_newLine}\t\t3{_newLine}";
            ObjectPrinter.For<HashSet<int>>()
                         .PrintToString(new HashSet<int> {1, 2, 3})
                         .Should()
                         .BeEquivalentTo(result);
        }

        [Test]
        public void Throw_WhenExcludingByInvalidSelector()
        {
            Action exclusion = () => _printer.Excluding(p => p.ToString());
            exclusion.Should()
                     .Throw<ArgumentException>()
                     .WithMessage("Selector expression is invalid");
        }

        [Test]
        public void Throw_WhenObjectCausesInfiniteLoops()
        {
            var loop = new Lööps {Number = 3};
            loop.Loop = loop;
            Action printing = () => ObjectPrinter.For<Lööps>()
                                                 .PrintToString(loop);

            printing.Should()
                    .Throw<ArgumentException>();
        }
    }
}
