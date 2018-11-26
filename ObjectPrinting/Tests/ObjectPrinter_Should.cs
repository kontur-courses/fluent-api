using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    public class ObjectPrinter_Should
    {
        private PrintingConfig<Person> printer;
        private Person person;
        private string newLine;

        [SetUp]
        public void SetUp()
        {
            printer = ObjectPrinter.For<Person>();
            person = new Person
            {
                Age = 19,
                Height = 188.9,
                Name = "Alexander",
                Id = Guid.Empty
            };
            newLine = Environment.NewLine;
        }


        [Test]
        public void ExcludeTypes_BySelector()
        {
            var expectedResult =
                $"Person{newLine}	Id = Guid{newLine}	Name = Alexander{newLine}	Age = 19{newLine}";
            printer
                .Excluding<double>()
                .PrintToString(person)
                .Should().Be(expectedResult);
        }

        [Test]
        public void ExcludeProperties_BySelector()
        {
            var expectedResult =
                $"Person{newLine}	Name = Alexander{newLine}	Height = 188,9{newLine}	Age = 19{newLine}";
            printer
                .Excluding(p => p.Id)
                .PrintToString(person)
                .Should().Be(expectedResult);
        }

        [Test]
        public void SpecializeType_BySelector_ForInt()
        {
            var expectedResult =
                $"Person{newLine}	Id = Guid{newLine}	Name = Alexander{newLine}	Height = 188,9{newLine}	Age = int 19{newLine}";
            printer
                .Printing<int>()
                .Using(num => "int " + num.ToString())
                .PrintToString(person)
                .Should().Be(expectedResult);
        }

        [Test]
        public void SpecializeType_BySelector_ForDouble()
        {
            var expectedResult =
                $"Person{newLine}	Id = Guid{newLine}	Name = Alexander{newLine}	Height = double 188,9{newLine}	Age = 19{newLine}";
            printer
                .Printing<double>()
                .Using(num => "double " + num.ToString())
                .PrintToString(person)
                .Should().Be(expectedResult);
        }

        [TestCase("es-ES", "188,9", TestName = "ES culture")]
        [TestCase("es-US", "188.9", TestName = "US culture")]
        public void Serialize_WithAnotherCulture(string cultureCode, string number)
        {
            var expectedResult =
                $"Person{newLine}	Id = Guid{newLine}	Name = Alexander{newLine}	Height = {number}{newLine}	Age = 19{newLine}";
            printer
                .Printing<double>()
                .Using(CultureInfo.CreateSpecificCulture(cultureCode))
                .PrintToString(person)
                .Should().Be(expectedResult);
        }

        [Test]
        public void SpecializeProperties_BySelector()
        {
            var expectedResult =
                $"Person{newLine}	Id = Guid{newLine}	Name = Alexander{newLine}	Height = 188,9 sm{newLine}	Age = 19{newLine}";
            printer
                .Printing(p => p.Height)
                .Using(height => height.ToString() + " sm")
                .PrintToString(person)
                .Should().Be(expectedResult);
        }

        [Test]
        public void PrintStrings_WithTrimming()
        {
            var expectedResult =
                $"Person{newLine}	Id = Guid{newLine}	Name = Alex{newLine}	Height = 188,9{newLine}	Age = 19{newLine}";
            printer
                .Printing<string>()
                .Trim(4)
                .PrintToString(person)
                .Should().Be(expectedResult);
        }

        [Test]
        public void Print_Default()
        {
            var expectedResult =
                $"Person{newLine}	Id = Guid{newLine}	Name = Alexander{newLine}	Height = 188,9{newLine}	Age = 19{newLine}";
            printer
                .PrintToString(person)
                .Should().Be(expectedResult);
        }

        [Test]
        public void UseLastSpecifiedConfig()
        {
            var expectedResult =
                $"Person{newLine}	Id = Guid{newLine}	Name = c{newLine}	Height = 188,9{newLine}	Age = 19{newLine}";
            printer
                .Printing(p => p.Name)
                .Using(name => "a")
                .Printing(p => p.Name)
                .Using(name => "b")
                .Printing(p => p.Name)
                .Using(name => "c")
                .PrintToString(person)
                .Should().Be(expectedResult);
        }

        [Test]
        public void Print_NestingTypes()
        {
            var linkedList = new LinkedListNode
            {
                Value = 1,
                Next = new LinkedListNode
                {
                    Value = 2
                }
            };
            var printer = ObjectPrinter.For<LinkedListNode>();

            var expectedResult =
                $"LinkedListNode{newLine}	Value = 1{newLine}	Next = LinkedListNode{newLine}		Value = 2{newLine}		Next = null{newLine}";

            printer.PrintToString(linkedList).Should().Be(expectedResult);
        }
    }
}