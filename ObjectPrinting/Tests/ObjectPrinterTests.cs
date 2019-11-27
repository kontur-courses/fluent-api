using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        private Person person = new Person {Name = "Vasa", Id = new Guid(), Age = 19, Height = 170.1};
        [Test]
        public void ExcludingCertainTypeOfProperty_Int()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<int>();
            var actual = printer.PrintToString(person);

            actual.Should().Be("Person\r\n\tId = Guid\r\n\tName = Vasa\r\n\tSecondName = null\r\n\tHeight = 170,1\r\n");
        }
        
        [Test]
        public void ExcludingCertainTypeOfProperty_String()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<string>();
            var actual = printer.PrintToString(person);

            actual.Should().Be("Person\r\n\tId = Guid\r\n\tHeight = 170,1\r\n\tAge = 19\r\n");
        }
        
        [Test]
        public void ExcludingCertainTypeOfProperty_SeveralTypes()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<string>()
                .Excluding<int>()
                .Excluding<Guid>()
                .Excluding<double>();
            var actual = printer.PrintToString(person);

            actual.Should().Be("Person\r\n");
        }
        
        [Test]
        public void PrintingUsingAnotherSerialize_String()
        {
            var printer = ObjectPrinter.For<Person>().Printing<string>().Using(x => x.Substring(0, 2));
            var actual = printer.PrintToString(person);
            
            actual.Should().Be("Person\r\n\tId = Guid\r\n\tName = Va\r\n\tSecondName = null\r\n\tHeight = 170,1\r\n\tAge = 19\r\n");
        }
        
        [Test]
        public void PrintingUsingAnotherSerialize_int()
        {
            var printer = ObjectPrinter.For<Person>().Printing<int>().Using(x => "X");
            var actual = printer.PrintToString(person);
            
            actual.Should().Be("Person\r\n\tId = Guid\r\n\tName = Vasa\r\n\tSecondName = null\r\n\tHeight = 170,1\r\n\tAge = X\r\n");
        }
        
        [Test]
        public void PrintingUsingAnotherSerialize_SeveralTypes()
        {
            var person = new Person {Name = "Vasa", SecondName = "As", Id = new Guid(), Age = 19, Height = 170.0};

            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(x => "X")
                .Printing<string>().Using(x => "NoName");
            var actual = printer.PrintToString(person);
            
            actual.Should().Be("Person\r\n\tId = Guid\r\n\tName = NoName\r\n\tSecondName = NoName\r\n\tHeight = 170\r\n\tAge = X\r\n");
        }
        
        [Test]
        public void PrintingUsingAnotherCultureInfo_Double()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(CultureInfo.InvariantCulture);
            var actual = printer.PrintToString(person);

            actual.Should().Be("Person\r\n\tId = Guid\r\n\tName = Vasa\r\n\tSecondName = null\r\n\tHeight = 170.1\r\n\tAge = 19\r\n");
        }
        
        [Test]
        public void PrintingUsingSerializedOfProperty_Name()
        {
            var person = new Person {Name = "Vasa", SecondName = "Zip", Id = new Guid(), Age = 19, Height = 170.1};

            var printer = ObjectPrinter.For<Person>()
                .Printing(x => x.Name).Using(x => x.Substring(0, 2));
            var actual = printer.PrintToString(person);
            
            actual.Should().Be("Person\r\n\tId = Guid\r\n\tName = Va\r\n\tSecondName = Zip\r\n\tHeight = 170,1\r\n\tAge = 19\r\n");
        }
        
        [Test]
        public void PrintingTrimToLength_String()
        {
            var person = new Person {Name = "Vasa", SecondName = "Zip", Id = new Guid(), Age = 19, Height = 170.1};

            var printer = ObjectPrinter.For<Person>()
                .Printing<string>().TrimmedToLength(2);
            var actual = printer.PrintToString(person);
            
            actual.Should().Be("Person\r\n\tId = Guid\r\n\tName = Va\r\n\tSecondName = Zi\r\n\tHeight = 170,1\r\n\tAge = 19\r\n");
        }
        
        [Test]
        public void PrintingUsingTrimToLength_Name()
        {
            var person = new Person {Name = "Vasa", SecondName = "Zip", Id = new Guid(), Age = 19, Height = 170.1};

            var printer = ObjectPrinter.For<Person>()
                .Printing(x => x.Name).TrimmedToLength(2);
            var actual = printer.PrintToString(person);
            
            actual.Should().Be("Person\r\n\tId = Guid\r\n\tName = Va\r\n\tSecondName = Zip\r\n\tHeight = 170,1\r\n\tAge = 19\r\n");
        }

        [Test]
        public void ExcludingProperty_Name()
        {
            var printer = ObjectPrinter.For<Person>().Excluding(x => x.Name);
            var actual = printer.PrintToString(person);
            
            actual.Should().Be("Person\r\n\tId = Guid\r\n\tSecondName = null\r\n\tHeight = 170,1\r\n\tAge = 19\r\n");
        }
        
        [Test]
        public void ExcludingProperty_Name_Age()
        {
            var printer = ObjectPrinter.For<Person>().Excluding(x => x.Name)
                .Excluding(x => x.Age);
            var actual = printer.PrintToString(person);
            
            actual.Should().Be("Person\r\n\tId = Guid\r\n\tSecondName = null\r\n\tHeight = 170,1\r\n");
        }

        [Test]
        public void NoStackOverfull_IfObjectContainsThemSelf()
        {
            var circle = new Circle {Radius = 12};
            circle.Self = circle;

            var printer = ObjectPrinter.For<Circle>();
            var actual = printer.PrintToString(circle);

            actual.Should().Be("Circle\r\n\tRadius = 12\r\n\tSelf = Circle\r\n");
        }

        [Test]
        public void Collection_WithAllMethod()
        {
            var personList = new List<Person>
            {
                person,
                new Person
                {
                    Name = "Bill",
                    SecondName = "Vil",
                    Id = new Guid(),
                    Age = 18,
                    Height = 123
                }
            };

            var printer = ObjectPrinter.For<List<Person>>()
                .Excluding<string>()
                .Excluding(list => list[0].Id)
                .Printing<int>().Using(x => "XX")
                .Printing(list => list[0].Height).Using(x => "AAA");
            var actual = printer.PrintToString(personList);

            actual.Should().Be("List<Person>" +
                               "\r\n\tPerson" +
                               "\r\n\t\tHeight = AAA" +
                               "\r\n\t\tAge = XX" +
                               "\r\n\tPerson" +
                               "\r\n\t\tHeight = AAA" +
                               "\r\n\t\tAge = XX\r\n");
        }
    }
}