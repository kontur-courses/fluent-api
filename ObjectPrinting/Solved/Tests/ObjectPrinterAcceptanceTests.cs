using System;
using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting.Solved.Tests
{
    public class ObjectPrinterAcceptanceTests
    {
        private readonly Person person = new Person {Name = "Alex", Age = 19};
        private PrintingConfig<Person> printer;

        [SetUp]
        public void SetUp()
        {
            printer = ObjectPrinter.For<Person>();
        }

        [Test]
        public void Demo()
        {
            printer
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => (i + 1).ToString())
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(10)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Age);

            var s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            var s2 = person.PrintToString();

            //8. ...с конфигурированием
            var s3 = person.PrintToString(s => s.Excluding(p => p.Age));
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }

        [Test]
        public void PrintToString_CorrectResult_WhenExcludingInt()
        {
            printer.Excluding<int>();
            Assert.AreEqual("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 0\r\n",
                printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenExcludingDouble()
        {
            printer.Excluding<double>();
            Assert.AreEqual("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tAge = 19\r\n", printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenExcludingGuid()
        {
            printer.Excluding<Guid>();
            Assert.AreEqual("Person\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = 19\r\n", printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingInt()
        {
            printer.Printing<int>().Using(i => (i + 1).ToString());
            Assert.AreEqual("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = 20",
                printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingString()
        {
            printer.Printing<string>().Using(i => i + i);
            Assert.AreEqual("Person\r\n\tId = Guid\r\n\tName = AlexAlex\tHeight = 0\r\n\tAge = 19\r\n",
                printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingGuid()
        {
            printer.Printing<Guid>().Using(i => "Guid");
            Assert.AreEqual("Person\r\n\tId = Guid\tName = Alex\r\n\tHeight = 0\r\n\tAge = 19\r\n",
                printer.PrintToString(person));
        }
    }
}