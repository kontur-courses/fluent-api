using System;
using NUnit.Framework;

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
        public void PrintToString_PrintsPublicProperties()
        {
            var person = new Person
            {
                Name = "Alex",
                Age = 19
            };

            var result = ObjectPrinter.For<Person>()
                .PrintToString(person);

            Assert.That(result, Does.Contain("Name = Alex"));
            Assert.That(result, Does.Contain("Age = 19"));
        }

        [Test]
        public void PrintToString_PrintsPublicFields()
        {
            var person = new Person
            {
                Score = 73
            };

            var result = ObjectPrinter.For<Person>()
                .PrintToString(person);

            Assert.That(result, Does.Contain("Score = 73"));
        }

        [Test]
        public void ExcludingType_DoesNotPrintMembersOfThatType()
        {
            var person = new Person
            {
                Name = "Alex",
                Age = 19,
                Id = Guid.NewGuid(),
                ExternalId = Guid.NewGuid()
            };

            var result = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .PrintToString(person);

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Not.Contain("Id = "));
                Assert.That(result, Does.Not.Contain("ExternalId = "));

                Assert.That(result, Does.Contain("Name = Alex"));
                Assert.That(result, Does.Contain("Age = 19"));
            });
        }

        [Test]
        public void ExcludingType_DoesNotInvokeExcludedPropertyGetter()
        {
            var person = new PersonWithTrackedId();

            var result = ObjectPrinter.For<PersonWithTrackedId>()
                .Excluding<Guid>()
                .PrintToString(person);

            Assert.That(person.IdGetterCalls, Is.Zero);
            Assert.That(result, Does.Not.Contain("Id = "));
        }

        private sealed class PersonWithTrackedId
        {
            public int IdGetterCalls { get; private set; }

            public Guid Id
            {
                get
                {
                    IdGetterCalls++;
                    return Guid.NewGuid();
                }
            }
        }
    }
}