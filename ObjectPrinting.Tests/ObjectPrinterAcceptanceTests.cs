using System;
using System.Globalization;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Tests
{
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void AcceptanceTest()
        {
            var person = new Person { FirstName = "Bob", LastName = "Martin", Age = 20 };
            person.Parent = person;

            var expectedResult = new StringBuilder()
                .AppendLine("Person")
                .AppendLine("\tLastName = Mar")
                .AppendLine("\tId = \"00000000-0000-0000-0000-000000000000\"")
                .AppendLine("\tFirstName = cool story Bob")
                .AppendLine("\tHeight = 0")
                .AppendLine("\tParent = ![Cyclic reference]!")
                .ToString();
            
            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Exclude<int>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Use<Guid>().With(x => $"\"{x}\"")
                //3. Для числовых типов указать культуру
                .Use<double>().With(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .Use(x => x.FirstName).With(x => $"cool story {x}")
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Use(x => x.LastName).With(3)
                //6. Исключить из сериализации конкретного свойства
                .Exclude(x => x.Age)
                //7. Детект циклических ссылок
                .UseCycleReference();
            
            var printedObject = printer.PrintToString(person);
            printedObject.Should().Be(expectedResult);
        }
    }
}