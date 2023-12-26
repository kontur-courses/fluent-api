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
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 180.5, SubPerson = new SubPerson() };
            person.SubPerson.Age = 15;
            person.SubPerson.Person = person;

            var printer = ObjectPrinter.For<Person>();

            //Исключение из сериализации свойства/ поля определенного типа
            //Альтернативный способ сериализации для определенного типа
            //Для всех типов, имеющих культуру, есть возможность ее указать
            printer.Exclude<double>()
                .Using(p => p.Age, age => (age + 1000).ToString())
                .Using<double>(CultureInfo.InvariantCulture)
                .Using<string>((p) => p.ToUpper())
                .Trim(p => p.Name, 1)
                .Exclude(p => p.Name)
                .WithMaxRecursion(3);

            //Корректная обработка циклических ссылок между объектами(не должны приводить к StackOverflowException)
        }

        [Test]
        public void Do44Something_WhenSomething()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 160 };
            var printer = ObjectPrinter.For<Person>();
            
            var s1 = printer
                .Using(p => p.Name, n => n + ":))")
                .Trim(p => p.Name, 6)
                .PrintToString(person);

            s1.Should().Be("Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alex:)\r\n\tHeight = 160\r\n\tAge = 19\r\n\tSubPerson = null\r\n\tPublicField = null\r\n");
        }

        [Test]
        public void ShouldExcludeMember_WhenItsTypeSpecified()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>();
            printer.Exclude(p => p.Age)
                .Exclude<double>();

            var s1 = printer.PrintToString(person);
            s1.Should().Be(
                "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alex\r\n\tSubPerson = null\r\n\tPublicField = null\r\n");
        }

        [Test]
        public void ShouldUseTrimming_WhenItsSpecifiedForType()
        {
            var person = new Person { Name = "Petr", Age = 20, Height = 180 };
            var printer = ObjectPrinter.For<Person>();

            var s1 = printer.Trim(p => p.Name, 1).PrintToString(person);

            s1.Should().Be(
                "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = P\r\n\tHeight = 180\r\n\tAge = 20\r\n\tSubPerson = null\r\n\tPublicField = null\r\n");
        }

        [Test]
        public void ShouldSerializeMember_WithGivenFunc()
        {
            var person = new Person { Name = "Petr", Age = 20, Height = 180 };
            var printer = ObjectPrinter.For<Person>();

            var s1 = printer.Using(p => p.Age, age => (age + 1000).ToString()).PrintToString(person);

            s1.Should().Be(
                "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Petr\r\n\tHeight = 180\r\n\tAge = 1020\r\n\tSubPerson = null\r\n\tPublicField = null\r\n");
        }

        [Test]
        public void SetCulture_ShouldAddedCultureInfo()
        {
            var person = new Person { Name = "Petr", Age = 20, Height = 180.5 };
            var printer = ObjectPrinter.For<Person>();

            var s1 = printer.Using<double>(CultureInfo.InvariantCulture).PrintToString(person);

            s1.Should().Be(
                "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Petr\r\n\tHeight = 180.5\r\n\tAge = 20\r\n\tSubPerson = null\r\n\tPublicField = null\r\n");
        }

        [Test]
        public void WhenCyclicLinksWasFound_ShouldPrintWithRecursionLimit()
        {
            var person = new Person { Name = "Petr", Age = 20, Height = 180, SubPerson = new SubPerson() };
            person.SubPerson.Age = 15;
            person.SubPerson.Person = person;
            var printer = ObjectPrinter.For<Person>();

            var s1 = printer.PrintToString(person);

            s1.Should().Be(
                "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Petr\r\n\tHeight = 180\r\n\tAge = 20\r\n\tSubPerson = SubPerson\r\n\t\tPerson = Maximum recursion has been reached\r\n\t\tAge = 15\r\n\tPublicField = null\r\n");
        }

        [Test]
        public void WhenPassNull_ShouldReturnNullString()
        {
            var printer = ObjectPrinter.For<Person>();
            var s1 = printer.PrintToString(null);

            s1.Should().Be("null\r\n");
        }

        [Test]
        public void WhenPassFinalType_ShouldReturnStringRepresentationOfThisType()
        {
            var printer = ObjectPrinter.For<int>();
            var s1 = printer.PrintToString(1);

            s1.Should().Be("1\r\n");
        }

        [Test]
        public void DoSomething_WhenSomething()
        {
            var printer = ObjectPrinter.For<int>();
            var s1 = printer.PrintToString(1);

            s1.Should().Be("1\r\n");
        }
    }
}