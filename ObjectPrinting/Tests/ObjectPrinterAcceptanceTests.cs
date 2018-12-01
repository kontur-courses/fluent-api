using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting.Solved.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void CulterinfoTest()
        {
            var person = new Person {Height = 3.28};
            Console.WriteLine(person.PrintToString(p => p.Printing<double>().Using(new CultureInfo("de"))));
            Console.WriteLine(person.PrintToString(p => p));
        }

        [Test]
        public void Demo()
        {
            var person = new Person
            {
                Age = 19, Name = "AlexAlexAlex", Height = 3, List = new List<double>(new[] {1d, 2, 3, 4})
            };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString())
                //3. Для числовых типов указать культуру
                .Printing<int>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Height).Using(p => (p + 3).ToString())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(10)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Height);


            var s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            var s2 = person.PrintToString();

            //8. ...с конфигурированием
            var s3 = person.PrintToString();
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }

        [Test]
        public void IEnumerablePrintigTest()
        {
            var list = new List<int>(new[] {1, 2, 3, 4, 5});
            Console.WriteLine(list.PrintToString(p => p));
        }

        [Test]
        public void NestingLevelLoopTest()
        {
            var token1 = new Token();
            var token2 = new Token();
            token1.Next = token2;
            token2.Next = token1;

            Console.WriteLine(token1.PrintToString(p => p.WithNestingLevel(5)));
        }

        [Test]
        public void PrimitivePrintingTest()
        {
            var number = 3;
            Console.WriteLine(number.PrintToString(p => p));
        }
    }
}