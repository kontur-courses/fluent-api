using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Infrastructure;
using ObjectPrintingTests.Mocks;

namespace ObjectPrintingTests
{
    public class Tests
    {
        private PrintingConfig<object> printer;
        
        [SetUp]
        public void Setup()
        {
            printer = ObjectPrinter.For<object>();
        }

        [Test]
        public void TypeExcluding_Exists()
        {
            var _ = printer.Excluding<string>();
        }

        [Test]
        public void SelectorForPropertyOrField_Exists()
        {
            var _ = printer
                .Printing<object>();
        }

        [Test]
        public void AlternateSerializationForType_Exists()
        {
            var _ = printer
                .Printing<int>()
                .Using(i => "i");
        }

        [Test]
        public void CultureInfoForIFormattable_Exists()
        {
            var _ = printer
                .Printing<double>()
                .Using(CultureInfo.InvariantCulture);
        }

        [Test]
        public void StringShortening_Exists()
        {
            var _ = printer
                .Printing<string>()
                .TrimmedToLength(10);
        }

        [Test]
        public void PropertyOrFieldExcluding_Exists()
        {
            var printer = ObjectPrinter.For<SingleProperty>();

            var _ = printer.Excluding(p => p.Property);
        }
        
        [Test]
        public void PrintToString_ExcludingByProperty()
        {
            var expected = GetSystemIndependent("Person\n\tName = name\n");
            var objectToPrint = new Person { Name = "name", Age = 10 };
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Age);

            var actual = printer.PrintToString(objectToPrint);

            actual.Should().Be(expected);
        }
        
        [Test]
        public void PrintToString_ExcludingByType()
        {
            var expected = GetSystemIndependent("Person\n\tName = name\n");
            var objectToPrint = new Person { Name = "name", Age = 10 };
            var printer = ObjectPrinter.For<Person>()
                .Excluding<int>();

            var actual = printer.PrintToString(objectToPrint);

            actual.Should().Be(expected);
        }

        private static string GetSystemIndependent(string text)
        {
            return text.Replace("\n", Environment.NewLine);
        }

        [Test]
        public void PrintToString_AlternateSerializationForType()
        {
            var expected = GetSystemIndependent("WithInteger\n\tInteger = i\n");
            var objectToPrint = new WithInteger();
            var _ = ObjectPrinter.For<WithInteger>()
                .Printing<int>()
                .Using(i => "i");

            var actual = printer.PrintToString(objectToPrint);

            actual.Should().Be(expected);
        }
        
        [Test]
        public void PrintToString_AlternateSerializationForProperty()
        {
            var expected = GetSystemIndependent("WithInteger\n\tInteger = i\n");
            var objectToPrint = new WithInteger();
            var _ = ObjectPrinter.For<WithInteger>()
                .Printing(p => p.Integer)
                .Using(i => "i");

            var actual = printer.PrintToString(objectToPrint);

            actual.Should().Be(expected);
        }
        
        [Test]
        public void PrintToString_WithCultureInfo()
        {
            var objectToPrint = new WithDouble();
            var _ = ObjectPrinter.For<WithDouble>()
                .Printing<double>()
                .Using(CultureInfo.CurrentCulture);
            var expected = GetSystemIndependent("WithDouble\n\tDouble = 0.0\n");
            
            var actual = printer.PrintToString(objectToPrint);

            actual.Should().Be(expected);
        }

        [Test]
        public void PrintToString_FormatProperty()
        {
            var objectToPrint = new SingleProperty {Property = null};
            var _ = ObjectPrinter.For<SingleProperty>()
                .Printing(p => p.Property)
                .Using(p => "custom");
            var expected = GetSystemIndependent("WithString\n\tProperty = custom\n");
            
            var actual = printer.PrintToString(objectToPrint);

            actual.Should().Be(expected);
        }
        
        [Test]
        public void PrintToString_WithStringShortening()
        {
            var objectToPrint = new WithString {String = "1234"};
            var _ = ObjectPrinter.For<WithString>()
                .Printing<string>()
                .TrimmedToLength(1);
            var expected = GetSystemIndependent("WithString\n\tString = 1\n");
            
            var actual = printer.PrintToString(objectToPrint);

            actual.Should().Be(expected);
        }

        [Test]
        public void PrintToString_ExcludeByName()
        {
            var objectToPrint = new Person();
            var _ = ObjectPrinter.For<Person>()
                .Excluding(p => p.Name);
            var expected = GetSystemIndependent("Person\n\tAge = 0\n");
            
            var actual = printer.PrintToString(objectToPrint);

            actual.Should().Be(expected);
        }

        [Test]
        public void PrintToString_DoesNotThrowWhenCycleReference()
        {
            var node1 = new Node();
            var node2 = new Node {Next = node1};
            node1.Next = node2;
            var objectToPrint = node1;
            var _ = ObjectPrinter.For<Node>();
            var expected = GetSystemIndependent("Node\n\tNext = Node\n\t\tNext = [cycle]\n");
            
            var actual = printer.PrintToString(objectToPrint);

            actual.Should().Be(expected);
        }
    }
}