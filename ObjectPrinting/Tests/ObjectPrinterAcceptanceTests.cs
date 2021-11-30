using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private PrintingConfig<Person> printer;
        private Person person;

        [SetUp]
        public void SetUp()
        {
            printer = ObjectPrinter.For<Person>();
            person = new Person
            {
                Name = "Albert",
                Age = 54,
                Height = 146,
                Id = new Guid()
            };
        }


        [Test]
        public void Demo()
        {
            var person = new Person {Name = "Alex", Age = 19};

            var printer = ObjectPrinter.For<Person>();
            //1. Исключить из сериализации свойства определенного типа
            //2. Указать альтернативный способ сериализации для определенного типа
            //3. Для числовых типов указать культуру
            //4. Настроить сериализацию конкретного свойства
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            //6. Исключить из сериализации конкретного свойства

            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }

        [Test]
        public void Excluding_ShouldExcludeSpecificTypeMembers()
        {
            var trianglePrinter = ObjectPrinter.For<Triangle>();
            var triangle = new Triangle();
            trianglePrinter
                .Excluding<double>()
                .PrintToString(triangle)
                .Should()
                .NotContain("B")
                .And.NotContain("C")
                .And.Contain("A = 1");
        }

        [Test]
        public void Printing_Using_ShouldReplaceSpecialTypeSerialization()
        {
            printer
                .Printing<int>().Using(x => $"{x + 1}")
                .PrintToString(person)
                .Should()
                .Contain($"Age = {person.Age + 1}");
        }

        [Test]
        public void Printing_Using_ShouldSpecifyNewCulture()
        {
            // не придумал проверки  :)
            printer
                .Printing<double>().Using(CultureInfo.GetCultureInfo("ru-Ru"))
                .PrintToString(person)
                .Should()
                .Contain(
                    $"\tHeight = {person.Height.ToString(CultureInfo.GetCultureInfo("ru-Ru"))}{Environment.NewLine}"
                );
        }

        [Test]
        public void Printing_Using_ShouldReplaceSpecialPropertySerialization()
        {
            var trianglePrinter = ObjectPrinter.For<Triangle>();
            var triangle = new Triangle();
            trianglePrinter
                .Printing(r => r.A).Using(x => "123")
                .PrintToString(triangle)
                .Should()
                .Contain("A = 123")
                .And.Contain("B = 1")
                .And.Contain("C = 1");
        }
        
        [Test]
        public void Printing_Using_ShouldReplaceSpecialFieldSerialization()
        {
            var rectPrinter = ObjectPrinter.For<Rectangle>();
            var rect = new Rectangle();
            rectPrinter
                .Printing(r => r.Height).Using(x => "123")
                .PrintToString(rect)
                .Should()
                .Contain("Height = 123")
                .And.Contain("Width = 4");
        }
    }
}