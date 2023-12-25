using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Tests;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void WhenExcludeType_ShouldReturnWithoutThisType()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>();
            printer.Exclude(p => p.Age)
                .Exclude<double>();

            //1. Исключить из сериализации свойства определенного типа
            //2. Указать альтернативный способ сериализации для определенного типа
            //3. Для числовых типов указать культуру
            //4. Настроить сериализацию конкретного свойства
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            //6. Исключить из сериализации конкретного свойства

            var s1 = printer.PrintToString(person);
            s1.Should().Be(
                "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alex\r\n\tSubPerson = null\r\n");

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }

        [Test]
        public void TrimString_ShouldReturnSubstring()
        {
            var person = new Person { Name = "Petr", Age = 20, Height = 180 };
            var printer = ObjectPrinter.For<Person>();

            var s1 = printer.Trim(p => p.Name, 1).PrintToString(person);

            s1.Should().Be(
                "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = P\r\n\tHeight = 180\r\n\tAge = 20\r\n\tSubPerson = null\r\n");
        }

        [Test]
        public void ShouldSerializeMember_WithGivenFunc()
        {
            var person = new Person { Name = "Petr", Age = 20, Height = 180 };
            var printer = ObjectPrinter.For<Person>();

            var s1 = printer.SerializeWith(person => person.Age, age => (age + 1000).ToString()).PrintToString(person);

            s1.Should().Be(
                "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Petr\r\n\tHeight = 180\r\n\tAge = 1020\r\n\tSubPerson = null\r\n");
        }

        [Test]
        public void SetCulture_ShouldAddedCultureInfo()
        {
            var person = new Person { Name = "Petr", Age = 20, Height = 180.5 };
            var printer = ObjectPrinter.For<Person>();

            var s1 = printer.SetCulture<double>(CultureInfo.InvariantCulture).PrintToString(person);

            s1.Should().Be(
                "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Petr\r\n\tHeight = 180.5\r\n\tAge = 20\r\n\tSubPerson = null\r\n");
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
                "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Petr\r\n\tHeight = 180\r\n\tAge = 20\r\n\tSubPerson = SubPerson\r\n\t\tPerson = Maximum recursion has been reached\r\n\t\tAge = 15\r\n");
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
    }
}