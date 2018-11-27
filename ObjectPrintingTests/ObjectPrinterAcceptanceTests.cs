using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void ExcludeFieldType()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>();

            printer.PrintToString(person)
                .ShouldBeEquivalentTo("Person\r\n		Name = Alex\r\n	Height = 0\r\n	Age = 19\r\n");
        }

        [Test]
        public void ExcludeFieldByName()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Id);

            printer.PrintToString(person)
                .ShouldBeEquivalentTo("Person\r\n		Name = Alex\r\n	Height = 0\r\n	Age = 19\r\n");
        }

        [Test]
        public void AlternativePrintingByType()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(x => "x");

            printer.PrintToString(person)
                .ShouldBeEquivalentTo("Person\r\n	Id = Guid\r\n	Name = Alex\r\n	Height = 0\r\n	Age = x\r\n");
        }

        [Test]
        public void AlternativePrintingByPropertyName()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .Printing(x => x.Age).Using(x => "x");

            printer.PrintToString(person)
                .ShouldBeEquivalentTo("Person\r\n	Id = Guid\r\n	Name = Alex\r\n	Height = 0\r\n	Age = x\r\n");
        }

        [Test]
        public void AlternativeCultureInfo()
        {
            var person = new Person { Name = "Alex", Height = 1.2 };

            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(CultureInfo.GetCultureInfo("en-UK"));

            printer.PrintToString(person)
                .ShouldBeEquivalentTo("Person\r\n	Id = Guid\r\n	Name = Alex\r\n	Height = 1.2\r\n	Age = 0\r\n");
        }

        [Test]
        public void TrimmingOfLongStrings()
        {
            var person = new Person { Name = "Alex" };

            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(1);

            printer.PrintToString(person)
                .ShouldBeEquivalentTo("Person\r\n	Id = Guid\r\n	Name = A\r\n	Height = 0\r\n	Age = 0\r\n");
        }

        [Test]
        public void TestArrayPrinting()
        {
            var list = new[] { 1, 2, 3, 4, 5 };
            list.PrintToString().ShouldBeEquivalentTo("Int32[]\r\n	0: 1\r\n	1: 2\r\n	2: 3\r\n	3: 4\r\n	4: 5\r\n");
        }

        [Test]
        public void ShouldNotThrow_AfterPrintingNullObject()
        {
            Action act = () => ((object)null).PrintToString();
            act.ShouldNotThrow();
        }

        internal class X
        {
            public X x { get; set; }
        }

        [Test]
        public void ShouldNotThrow_AfterInfinityNestingPrinting()
        {
            var x = new X();
            x.x = x;

            Action act = () => { x.PrintToString(); };

            act.ShouldNotThrow();
        }

        internal class Y
        {
            public Y(int y)
            {
                this.y = y;
            }

            private int y;
        }

        [Test]
        public void ShouldPrintPrivateProperties()
        {
            new Y(0).PrintToString().ShouldBeEquivalentTo("Y\r\n	y = 0\r\n");
        }
    }
}