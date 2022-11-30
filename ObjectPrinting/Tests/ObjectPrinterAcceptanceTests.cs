using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private Person person;
        private PrintingConfig<Person> printer;

        [SetUp]
        public void SetUp()
        {
            person = new Person { Name = "Alex", Age = 19, Double = 2.2 };
            
            printer = ObjectPrinter.For<Person>();
        }

        [Test]
        public void ExcludeType()
        {
            var res = printer
                .Exclude<string>()
                .PrintToString(person);
            var expected = string.Join(Environment.NewLine,
                "Person", "\tId = 00000000-0000-0000-0000-000000000000", "\tHeight = 0", "\tAge = 19", "\tDouble = 2,2", "\tParent = null", "");

            res.Should().Be(expected);
        }

        [Test]
        public void ExcludeName()
        {
            var res = printer
                .Exclude(e => e.Id)
                .PrintToString(person);
            var expected = string.Join(Environment.NewLine,
                 "Person", "\tName = Alex", "\tHeight = 0", "\tAge = 19", "\tDouble = 2,2", "\tParent = null", "");

            res.Should().Be(expected);
        }

        [Test]
        public void ChangeSerializationForType()
        {
            var res = printer
                .For<string>()
                    .SetSerialization(s => "строка с кастомной сериализацией")
                .PrintToString(person);
            var expected = string.Join(Environment.NewLine,
                "Person", "\tId = 00000000-0000-0000-0000-000000000000", "\tName = строка с кастомной сериализацией", "\tHeight = 0", "\tAge = 19"
                , "\tDouble = 2,2", "\tParent = null", "");

            res.Should().Be(expected);
        }

        [Test]
        public void ChangeSerializationForProperty()
        {
            var res = printer
                .For(e => e.Age)
                    .SetSerialization(num => "Изменил только Age")
                .PrintToString(person);
            var expected = string.Join(Environment.NewLine,
                "Person", "\tId = 00000000-0000-0000-0000-000000000000", "\tName = Alex", "\tHeight = 0", "\tAge = Изменил только Age", "\tDouble = 2,2"
                , "\tParent = null", "");

            res.Should().Be(expected);
        }

        [Test]
        public void TrimString()
        {
            var res = printer
                .For(e => e.Name)
                .TrimmedToLength(1)
                .PrintToString(person);
            var expected = string.Join(Environment.NewLine,
                "Person", "\tId = 00000000-0000-0000-0000-000000000000", "\tName = A", "\tHeight = 0", "\tAge = 19", "\tDouble = 2,2", "\tParent = null", "");

            res.Should().Be(expected);
        }

        [Test]
        public void ChangeCulture()
        {
            var res = printer
                .For<double>()
                    .Using(System.Globalization.CultureInfo.GetCultureInfo("en-US"))
                .PrintToString(person);
            var expected = string.Join(Environment.NewLine,
                "Person", "\tId = 00000000-0000-0000-0000-000000000000", "\tName = Alex", "\tHeight = 0", 
                    "\tAge = 19", "\tDouble = 2.2", "\tParent = null", "");

            res.Should().Be(expected);
        }

        [Test]
        public void BrakeOnLooping()
        {
            person.Parent = new Person {Parent = person};
            var res = printer
                .PrintToString(person);
            var expected = string.Join(Environment.NewLine,
                "Person", "\tId = 00000000-0000-0000-0000-000000000000", "\tName = Alex", "\tHeight = 0", "\tAge = 19", "\tDouble = 2,2", "\tParent = Person",
                    "\t\t\tId = 00000000-0000-0000-0000-000000000000", "\t\t\tName = null", "\t\t\tHeight = 0", "\t\t\tAge = 0", "\t\t\tDouble = 0", "\t\t\tParent = this", "");

            res.Should().Be(expected);
        }

        [Test]
        public void WorkWithEnumerable()
        {
            var res1 = ObjectPrinter.For<int[]>().PrintToString(new[] {1, 2, 3, 4});
            var res2 = ObjectPrinter.For<List<int>>().PrintToString(new List<int> {1, 2, 3, 4});
            var res3 = ObjectPrinter.For<Dictionary<string, int>>().PrintToString(new Dictionary<string, int>
            {
                { "first", 10 },
                { "second", 0 },
                { "third", 43 },
            });
            var res4 = ObjectPrinter.For<List<Dictionary<int, int>>>()
                .PrintToString(new List<Dictionary<int, int>>
                {
                    new Dictionary<int, int> {{1, 10}, {2, 20}},
                });
            var expectedEnumerable = string.Join(Environment.NewLine,
                "", "\t0 = 1", "\t1 = 2", "\t2 = 3", "\t3 = 4", "");
            var expectedDictionary = string.Join(Environment.NewLine,
                "", "\tfirst = 10", "\tsecond = 0", "\tthird = 43", "");
            var expectedDictionaryInList = string.Join(Environment.NewLine,
                "", "\t0 = Dictionary`2", "\t\t\t1 = 10", "\t\t\t2 = 20", "");

            res1.Should().Be("Int32[]" + expectedEnumerable);
            res2.Should().Be("List`1" + expectedEnumerable);
            res3.Should().Be("Dictionary`2" + expectedDictionary);
            res4.Should().Be("List`1" + expectedDictionaryInList);
        }

        [Test]
        public void WorkWithOtherTypes()
        {
            var obj = new A {Date = DateTime.MinValue, Time = new TimeSpan()};
            var printer1 = ObjectPrinter.For<A>();
            var res = printer1.PrintToString(obj);
            var expected = string.Join(Environment.NewLine,
                "A", "\tDate = 01.01.0001 0:00:00", "\tTime = 00:00:00", "");

            res.Should().Be(expected);
        }

        [Test]
        public void CultureChangingUsingOnOnlyConfiguredProps()
        {
            var obj = new CultureChecker {Date = DateTime.MinValue, Double = 2.2};
            var printer = ObjectPrinter.For<CultureChecker>();
            var res = printer
                .For<DateTime>()
                    .Using(CultureInfo.GetCultureInfo("en-US"))
                .PrintToString(obj);
            var expected = string.Join(Environment.NewLine,
                "CultureChecker", "\tDate = 1/1/0001 12:00:00 AM", "\tDouble = 2,2", "");

            res.Should().Be(expected);
        }

        [Test]
        public void CorrectWorkDictionaryWithClassesInKeyAndValues()
        {
            var obj = new Dictionary<A, A>
            {
                {new A {Date = DateTime.MinValue, Time = new TimeSpan()}, 
                    new A {Date = DateTime.MinValue, Time = new TimeSpan()}}
            };
            var printer = ObjectPrinter.For<Dictionary<A, A>>();
            var res = printer.PrintToString(obj);
            var expected = string.Join(Environment.NewLine,
                "Dictionary`2", "\tObjectPrinting.Tests.A = A", "\t\t\tDate = 01.01.0001 0:00:00", "\t\t\tTime = 00:00:00", "");

            res.Should().Be(expected);
        }
    }
}