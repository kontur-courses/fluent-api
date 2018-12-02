using System;
using System.Globalization;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alexander", Age = 19, Height = 5.5, Weight = 58, LuckyNumbers = new [] { 3, 1, 45, 6, 0, -2, 23, 66, 1, 10, 11, 123 } };
            person.Parent = new Person { Child = person };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString("X"))
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //.Printing<double>().Using(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Weight).Using(w => w.ToString("D5"))
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(5)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.UselessField);

            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            string s2 = person.PrintToString();

            //8. ...с конфигурированием
            string s3 = person.PrintToString(s => s.Excluding(p => p.Age));
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);

            string s4 = new Person[] { new Person(), new Person() }.PrintToString();
            string s5 = new [] { 15, 22, 34, 34, 5}.PrintToString(); ;
            string s6 = 57.PrintToString();

            Console.WriteLine(s4);
            Console.WriteLine(s5);
            Console.WriteLine(s6);
        }

        [Test]
        public void TestNesting()
        {
            var a = new Node();
            var b = new Node();
            a.Left = b;
            a.Right = b;
            a.Left.Right = a;

            Console.WriteLine(a.PrintToString());
        }

        private class Node
        {
            public Node Left { get; set; }
            public Node Right { get; set; }
        }
    }
}