using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        [Test]
        public void ObjectPrinter_ExcludingCertainTypeOfProperty_Int()
        {
            var person = new Person {Name = "Vasa", Id = new Guid(), Age = 19, Height = 170.0};

            var printer = ObjectPrinter.For<Person>().Excluding<int>();
            var actual = printer.PrintToString(person);

            actual.Should().Be("Person\r\n\tId = Guid\r\n\tName = Vasa\r\n\tSecondName = null\r\n\tHeight = 170\r\n");
        }
        
        [Test]
        public void ObjectPrinter_ExcludingCertainTypeOfProperty_String()
        {
            var person = new Person {Name = "Vasa", Id = new Guid(), Age = 19, Height = 170.0};

            var printer = ObjectPrinter.For<Person>().Excluding<string>();
            var actual = printer.PrintToString(person);

            actual.Should().Be("Person\r\n\tId = Guid\r\n\tHeight = 170\r\n\tAge = 19\r\n");
        }
        
        [Test]
        public void ObjectPrinter_ExcludingCertainTypeOfProperty_SeveralTypes()
        {
            var person = new Person {Name = "Vasa", Id = new Guid(), Age = 19, Height = 170.0};

            var printer = ObjectPrinter.For<Person>().Excluding<string>()
                .Excluding<int>()
                .Excluding<Guid>()
                .Excluding<double>();
            var actual = printer.PrintToString(person);

            actual.Should().Be("Person\r\n");
        }
        
        [Test]
        public void ObjectPrinter_PrintingUsingAnotherSerialize_String()
        {
            var person = new Person {Name = "Vasa", Id = new Guid(), Age = 19, Height = 170.0};

            var printer = ObjectPrinter.For<Person>().Printing<string>().Using(x => x.Substring(0, 2));
            var actual = printer.PrintToString(person);
            
            actual.Should().Be("Person\r\n\tId = Guid\r\n\tName = Va\r\n\tSecondName = null\r\n\tHeight = 170\r\n\tAge = 19\r\n");
        }
        
        [Test]
        public void ObjectPrinter_PrintingUsingAnotherSerialize_int()
        {
            var person = new Person {Name = "Vasa", Id = new Guid(), Age = 19, Height = 170.0};

            var printer = ObjectPrinter.For<Person>().Printing<int>().Using(x => "X");
            var actual = printer.PrintToString(person);
            
            actual.Should().Be("Person\r\n\tId = Guid\r\n\tName = Vasa\r\n\tSecondName = null\r\n\tHeight = 170\r\n\tAge = X\r\n");
        }
        
        [Test]
        public void ObjectPrinter_PrintingUsingAnotherSerialize_SeveralTypes()
        {
            var person = new Person {Name = "Vasa", SecondName = "As", Id = new Guid(), Age = 19, Height = 170.0};

            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(x => "X")
                .Printing<string>().Using(x => "NoName");
            var actual = printer.PrintToString(person);
            
            actual.Should().Be("Person\r\n\tId = Guid\r\n\tName = NoName\r\n\tSecondName = NoName\r\n\tHeight = 170\r\n\tAge = X\r\n");
        }
        
        [Test]
        public void ObjectPrinter_PrintingUsingAnotherCultureInfo_Double()
        {
            var person = new Person {Name = "Vasa", Id = new Guid(), Age = 19, Height = 170.1};

            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(CultureInfo.CurrentUICulture);
            var actual = printer.PrintToString(person);

            actual.Should().Be("Person\r\n\tId = Guid\r\n\tName = Vasa\r\n\tSecondName = null\r\n\tHeight = 170,1\r\n\tAge = 19\r\n");
        }
        
        [Test]
        public void ObjectPrinter_PrintingUsingSerializedOfProperty_Name()
        {
            var person = new Person {Name = "Vasa", SecondName = "Zip", Id = new Guid(), Age = 19, Height = 170.1};

            var printer = ObjectPrinter.For<Person>()
                .Printing(x => x.Name).Using(x => x.Substring(0, 2));
            var actual = printer.PrintToString(person);
            
            Console.WriteLine(actual);
            actual.Should().Be("Person\r\n\tId = Guid\r\n\tName = Va\r\n\tSecondName = Zip\r\n\tHeight = 170,1\r\n\tAge = 19\r\n");
        }
    }
}