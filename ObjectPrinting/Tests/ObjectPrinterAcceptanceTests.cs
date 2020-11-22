using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [SetUp]
        public void SetUp()
        {
            person = new Person {Name = "Alex", Age = 19, Height = 195.5};
            personPrinter = ObjectPrinter.For<Person>();
        }

        private Person person;
        private PrintingConfig<Person> personPrinter;

        [Test]
        public void Demo()
        {
            person.Parent = new Person {Name = "Anna"};
            personPrinter = personPrinter
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString("X"))
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(10)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Age);

            var s1 = personPrinter.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            var s2 = person.PrintToString();

            //8. ...с конфигурированием
            var s3 = person.PrintToString(s => s.Excluding(p => p.Age));

            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }

        [Test]
        public void ObjectPrinter_ShouldReturnCorrectString_WithoutConfiguration()
        {
            person.PrintToString()
                .Should()
                .BeEquivalentTo(
                    "Person 1\r\n\tId = Guid 1\r\n\tName = Alex\r\n\tHeight = 195,5\r\n\tAge = 19\r\n\tParent = null\r\n");
        }

        [Test]
        public void ObjectPrinter_ShouldIgnorePropertyType_IfPropertyTypeIsExcluding()
        {
            personPrinter
                .Excluding<Guid>()
                .PrintToString(person)
                .Should().NotContain(nameof(person.Id));
        }

        [Test]
        public void ObjectPrinter_ShouldIgnoreProperty_IfPropertyIsExcluding()
        {
            personPrinter
                .Excluding(p => p.Age)
                .PrintToString(person)
                .Should().NotContain(nameof(person.Age));
        }

        [Test]
        public void ObjectPrinter_ShouldUseCustomTypePrinter()
        {
            personPrinter
                .Printing<string>()
                .Using(str => "name")
                .PrintToString(person)
                .Should().NotContain(person.Name).And.Contain("name");
        }

        [Test]
        public void ObjectPrinter_ShouldUseCulture()
        {
            var printerWithoutCulture = ObjectPrinter.For<Person>();
            var printerWithCustomCulture = personPrinter
                .Printing<double>()
                .Using(CultureInfo.InvariantCulture);
            printerWithoutCulture
                .PrintToString(person)
                .Should().Contain(person.Height.ToString());
            printerWithCustomCulture
                .PrintToString(person)
                .Should().Contain(person.Height.ToString(CultureInfo.InvariantCulture));
        }

        [Test]
        public void ObjectPrinter_ShouldUseCustomPropertyPrinter()
        {
            personPrinter.Printing(p => p.Age)
                .Using(age => "100")
                .PrintToString(person)
                .Should()
                .NotContain(nameof(person.Age) + " = " + person.Age)
                .And.Contain(nameof(person.Age) + " = 100");
        }

        [Test]
        public void ObjectPrinter_ShouldTrimStringProperties()
        {
            personPrinter
                .Printing(p => p.Name)
                .TrimmedToLength(1)
                .PrintToString(person)
                .Should().NotContain("Al").And.Contain("A");
        }

        [Test]
        public void ObjectPrinter_ShouldNotThrowStackOverflowException_OnObjectWithCircularReferences()
        {
            person.Parent = person;
            Action action = () => person.PrintToString();
            action.Should().NotThrow<StackOverflowException>();
        }
    }
}