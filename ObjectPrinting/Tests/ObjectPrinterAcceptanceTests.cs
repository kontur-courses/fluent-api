using System;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private Person simplePerson;
        private Person personWithParents;
        private Person personWithCyclingReference;
        private static readonly string NextProperty = $"\n\t";

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            simplePerson = new Person { Id = new Guid("0f8fad5b-d9cb-469f-a165-70867728950e"),Name = "simplePerson", Age = 45, Height = 180 };
            personWithParents = new Person() { Id=new Guid("7c9e6679-7425-40de-944b-e07fc1f90ae7"),Name = "personWithParents", Age = 20, Height = 175, Parents = new Person[]{simplePerson}};
            personWithCyclingReference = new Person { Id=new Guid("b1754c14-d296-4b0f-a09a-030017f4461f"),Name = "Cycle", Age = 34, Height = 192 };
            personWithCyclingReference.Parents = new Person[] { personWithCyclingReference };
        }

        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>();
            string s1 = printer.PrintToString(person);
        }

        [Test]
        public void ObjectPrinter_Should_Serialize_All_Properties_When_Setting_Are_Default()
        {
            var printer = ObjectPrinter.For<Person>();
            var actual = printer.PrintToString(simplePerson);

            actual.Should().Be($"Person Id = Guid Name = simplePerson Height = 180 Age = 45 Parents = null");
        }

        [Test]
        public void ObjectPrinter_Should_Excluding_PropertyType()
        {
            var printer = ObjectPrinter.For<Person>();
            var actual = printer.Exclude<int>().PrintToString(simplePerson);
            actual.Should().Be("Person Id = Guid Name = simplePerson Height = 180 Parents = null");
        }
        
        [Test]
        public void ObjectPrinter_Should_Excluding_Property()
        {
            var printer = ObjectPrinter.For<Person>();
            var actual = printer.Exclude<string>(person=>person.Name).PrintToString(simplePerson);
            actual.Should().Be("Person Id = Guid Height = 180 Age = 45 Parents = null");
        }


        //1. Исключить из сериализации свойства определенного типа
        //2. Указать альтернативный способ сериализации для определенного типа
        //3. Для числовых типов указать культуру
        //4. Настроить сериализацию конкретного свойства
        //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
        //6. Исключить из сериализации конкретного свойства
        //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
        //8. ...с конфигурированием
    }
}