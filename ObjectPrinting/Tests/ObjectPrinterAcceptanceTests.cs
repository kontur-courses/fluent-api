using FluentAssertions;
using NUnit.Framework;
using System;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19 };

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
        public void PrintToString_ExcludingType_PrintWothoutType()
        {
            var person = new Person() { Age = 19, Height = 180, Name = "Arseny" };

            var result = person.Print(config => config.ExcludingType<int>());

            result.Should().
                Contain("Name").And.
                NotContain("Age").And.Contain("Height");
        }

        [Test]
        public void Print_ExcludingMember_SkipsOnlyThatProperty()
        {
            var person = new Person { Age = 19, Height = 180, Name = "Arseny" };

            var result = person.Print(config => config.Excluding(p => p.Age));

            result.Should().Contain("Name = Arseny")
                .And.Contain("Height")
                .And.NotContain("Age");
        }

        [Test]
        public void Excluding_ThrowsArgumentException_WhenSelectorIsNotMemberAccess()
        {
            var config = ObjectPrinter.For<Person>();

            var result = config.Invoking(c =>
                         c.Excluding(p => p.Age + 1));
            
            result.Should().Throw<ArgumentException>();
        }
    }
}