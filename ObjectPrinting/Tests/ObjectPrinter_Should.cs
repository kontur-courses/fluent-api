using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Solved;
using ObjectPrinting.Solved.Tests;

namespace ObjectPrinting.Tests
{
    public class ObjectPrinter_Should
    {
        [Test]
        public void ObjectPrinter_ShouldPrint()
        {
            var person = new Person {Name = "Alex", Age = 19};
            ObjectPrinter
                .For<Person>()
                .PrintToString(person)
                .Should()
                .Be("Person\r\n	Id = Guid\r\n	Name = Alex\r\n	Height = 0\r\n	Age = 19\r\n");
        }

        [Test]
        public void ObjectPrinter_ShouldPrintExcludingByType()
        {
            var person = new Person {Name = "Alex", Age = 19};
            ObjectPrinter
                .For<Person>()
                .Excluding<Guid>()
                .PrintToString(person)
                .Should()
                .Be("Person\r\n	Name = Alex\r\n	Height = 0\r\n	Age = 19\r\n");
        }

        [Test]
        public void ObjectPrinter_ShouldPrintExcludingByName()
        {
            var person = new Person {Name = "Alex", Age = 19};
            ObjectPrinter
                .For<Person>()
                .Excluding(i => i.Age)
                .PrintToString(person)
                .Should()
                .Be("Person\r\n	Id = Guid\r\n	Name = Alex\r\n	Height = 0\r\n");
        }

        [Test]
        public void ObjectPrinter_ShouldPrintWithUsingAndPrinting()
        {
            var person = new Person {Name = "Alex", Age = 19};
            ObjectPrinter
                .For<Person>()
                .Printing<int>()
                .Using(i => "integer")
                .Printing<double>()
                .Using(i => "double")
                .PrintToString(person)
                .Should()
                .Be("Person\r\n	Id = Guid\r\n	Name = Alex\r\n	Height = double	Age = integer");
        }

        [Test]
        public void ObjectPrinter_ShouldPrintDoubleWithCultureInfo()
        {
            var person = new Person {Name = "Alex", Age = 19, Height = 7.1};
            ObjectPrinter
                .For<Person>()
                .Printing<double>()
                .Using(CultureInfo.CurrentCulture)
                .PrintToString(person)
                .Should()
                .Be("Person\r\n	Id = Guid\r\n	Name = Alex\r\n	Height = 7,1	Age = 19\r\n");
        }

        [Test]
        public void ObjectPrinter_ShouldPrintSelectedProperties()
        {
            var person = new Person {Name = "Alexandra", Age = 19, Height = 7.1};
            ObjectPrinter
                .For<Person>()
                .Printing(a => a.Name)
                .Using(a => "Just Alex")
                .PrintToString(person)
                .Should()
                .Be("Person\r\n	Id = Guid\r\n	Name = Just Alex	Height = 7,1\r\n	Age = 19\r\n");
        }

        [Test]
        public void ObjectPrinter_ShouldPrintTrimmedProperties()
        {
            var person = new Person {Name = "Alexandra", Age = 19, Height = 7.1};
            ObjectPrinter
                .For<Person>()
                .Printing(a => a.Name)
                .TrimmedToLength(5)
                .PrintToString(person)
                .Should()
                .Be("Person\r\n	Id = Guid\r\n	Name = Alexa	Height = 7,1\r\n	Age = 19\r\n");
        }

        [Test]
        public void ObjectPrinter_ShouldPrintComplicatedRequests()
        {
            var person = new Person {Name = "Alex", Age = 19, Height = 7.1};
            ObjectPrinter
                .For<Person>()
                .Printing<int>()
                .Using(a => Convert.ToString(a, 2))
                .Printing<double>()
                .Using(CultureInfo.InvariantCulture)
                .Excluding(a => a.Id)
                .Printing(a => a.Name)
                .TrimmedToLength(1)
                .PrintToString(person)
                .Should()
                .Be("Person\r\n	Name = A	Height = 7.1	Age = 10011");
        }

        [Test]
        public void ObjectPrinter_ShouldPrintCollections()
        {
            var persons = new[]
            {
                new Person {Name = "Alex", Age = 19, Height = 7.0},
                new Person {Name = "Alexandr", Age = 20, Height = 7.1}
            };
            ObjectPrinter.For<Person[]>()
                .PrintToString(persons)
                .Should()
                .Be(
                    "Person[]\r\n	Length = 2\r\n	LongLength = Int64\r\n	Rank = 1\r\n	" +
                    "IsReadOnly = Boolean\r\n	IsFixedSize = Boolean\r\n	IsSynchronized = Boolean\r\n	Children: 	" +
                    "Person\r\n	Id = Guid\r\n	Name = Alex\r\n	Height = 7\r\n	Age = 19\r\n	Person\r\n	Id = Guid\r\n	" +
                    "Name = Alexandr\r\n	Height = 7,1\r\n	Age = 20\r\n");
        }

        [Test]
        public void ObjectPrinter_ShouldPrintSimpleCollections()
        {
            var array = new[] {"Child1", "Child2", "Child3", "Child4", "Child5"};
            ObjectPrinter.For<string[]>()
                .PrintToString(array)
                .Should()
                .Be(
                    "String[]\r\n	Length = 5\r\n	LongLength = Int64\r\n	Rank = 1\r\n	IsReadOnly = Boolean\r\n	" +
                    "IsFixedSize = Boolean\r\n	IsSynchronized = Boolean\r\n	" +
                    "Children: 	Child1\r\n	Child2\r\n	Child3\r\n	Child4\r\n	Child5\r\n");
        }

        [Test]
        public void ObjectPrinter_ShouldPrintCollectionsWithRequests()
        {
            var persons = new[]
            {
                new Person {Name = "Alex", Age = 19, Height = 7.0},
                new Person {Name = "Alexandr", Age = 20, Height = 7.1}
            };
            ObjectPrinter
                .For<Person[]>()
                .Printing<double>()
                .Using(CultureInfo.CurrentCulture)
                .Printing<int>()
                .Using(a => Convert.ToString(a, 2))
                .PrintToString(persons)
                .Should()
                .Be("Person[]\r\n	Length = 10	LongLength = Int64\r\n	Rank = 1	" +
                    "IsReadOnly = Boolean\r\n	IsFixedSize = Boolean\r\n	IsSynchronized = Boolean\r\n	" +
                    "Children: 	Person\r\n	Id = Guid\r\n	Name = Alex\r\n	Height = 7	Age = 10011	Person\r\n	" +
                    "Id = Guid\r\n	Name = Alexandr\r\n	Height = 7,1	Age = 10100");
        }
    }
}