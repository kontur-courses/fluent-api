using System;
using System.Globalization;
using System.Collections.Generic;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19, SecondName = "Shmalex" };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2.Указать альтернативный способ сериализации для определенного типа
                .ChangePrintFor<string>().Using(s => s.Trim())
                //3. Для числовых типов указать культуру
                .ChangePrintFor<int>().Using(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                .ChangePrintFor(p => p.Name).Using(value => value.ToUpper())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .ChangePrintFor(p => p.Name).TrimToLength(5)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Name);

            var result = printer.PrintToString(person);
            Console.Write(result);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию    
            //8. ...с конфигурированием
        }

        [Test]
        public void Excluding_Type()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>();

            var result = printer.PrintToString(person);

            Assert.AreEqual("Person\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = 19\r\n\tSecondName = null\r\n", result);
        }

        [Test]
        public void ChangePrintFor_Type_Using_Function()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>()
                .ChangePrintFor<int>().Using(s => (s * 100).ToString());

            var result = printer.PrintToString(person);

            Assert.AreEqual("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = 1900\tSecondName = null\r\n", result);
        }

        [Test]
        public void ChangePrintFor_Int_Using_CultureInfo()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>()
                .ChangePrintFor<int>().Using(CultureInfo.CurrentCulture);

            var result = printer.PrintToString(person);

            Assert.AreEqual("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = ru-RU\tSecondName = null\r\n", result);
        }

        [Test]
        public void ChangePrintFor_Property_Using_Function()
        {
            var person = new Person { Name = "Alex", Age = 19 , SecondName = "Shmalex"};
            var printer = ObjectPrinter.For<Person>()
                .ChangePrintFor(p => p.Name).Using(value => value.ToUpper());

            var result = printer.PrintToString(person);

            Assert.AreEqual("Person\r\n\tId = Guid\r\n\tName = ALEX\tHeight = 0\r\n\tAge = 19\r\n\tSecondName = Shmalex\r\n", result);
        }

        [Test]
        public void ChangePrintFor_String_TrimToLength()
        {
            var person = new Person { Name = "Alex", Age = 19, SecondName = "Shmalex" };
            var printer = ObjectPrinter.For<Person>()
                .ChangePrintFor<string>().TrimToLength(3);

            var result = printer.PrintToString(person);

            Assert.AreEqual("Person\r\n\tId = Guid\r\n\tName = Ale\tHeight = 0\r\n\tAge = 19\r\n\tSecondName = Shm", result);
        }

        [Test]
        public void Excluding_Property()
        {
            var person = new Person { Name = "Alex", Age = 19, SecondName = "Shmalex" };
            var printer = ObjectPrinter.For<Person>().Excluding(p => p.SecondName);

            var result = printer.PrintToString(person);

            Assert.AreEqual("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = 19\r\n", result);
        }

        [Test]
        public void PrintToString_List()
        {
            var list = new List<int>() { 1, 2, 3 };
            var printer = ObjectPrinter.For<List<int>>();

            var result = printer.PrintToString(list);

            Assert.AreEqual("List`1\r\n\t1\r\n\t2\r\n\t3\r\n", result);
        }

        [Test]
        public void PrintToString_Dictionary()
        {
            var list = new Dictionary<int, string>() {
                { 1 , "a" },
                { 2 , "b" },
                { 3 , "c" }};
            var printer = ObjectPrinter.For<Dictionary<int, string>>();

            var result = printer.PrintToString(list);

            Assert.AreEqual(
                "Dictionary`2" +
                "\r\n\tKeyValuePair`2" +
                "\r\n\t\tKey = 1" +
                "\r\n\t\tValue = a" +
                "\r\n\tKeyValuePair`2" +
                "\r\n\t\tKey = 2" +
                "\r\n\t\tValue = b" +
                "\r\n\tKeyValuePair`2" +
                "\r\n\t\tKey = 3" +
                "\r\n\t\tValue = c" +
                "\r\n", result);
        }

        [Test]
        public void SetMaxNumberItems()
        {
            var list = new List<int>() { 1, 2, 3 };
            var printer = ObjectPrinter.For<List<int>>().SetMaxNumberListItems(2);

            var result = printer.PrintToString(list);

            Assert.AreEqual("List`1\r\n\t1\r\n\t2\r\n", result);
        }

        [Test]
        public void PrintToString_CircularLinks()
        {
            var person1 = new CyclicalPerson { Number = 1 };
            var person2 = new CyclicalPerson { Number = 2, Next = person1 };
            var person3 = new CyclicalPerson { Number = 3, Next = person2 };
            person1.Next = person3;
            var printer = ObjectPrinter.For<CyclicalPerson>().SetMaxNumberListItems(2);

            var result = printer.PrintToString(person1);

            Assert.AreEqual(
                "CyclicalPerson" +
                "\r\n\tNext = CyclicalPerson" +
                "\r\n\t\tNext = CyclicalPerson" +
                "\r\n\t\t\tNext = Is higher in the hierarchy by 2 steps" +
                "\r\n\t\t\tNumber = 2" +
                "\r\n\t\tNumber = 3" +
                "\r\n\tNumber = 1" +
                "\r\n", result);
        }
    }
}