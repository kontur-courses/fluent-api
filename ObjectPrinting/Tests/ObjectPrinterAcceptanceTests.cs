using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        private Person person;
        private A a;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            person = new Person {Name = "Alex", Age = 19, Height = 1.85, Score = new List<int> {1, 2, 3, 4, 5}, Dictionary = new Dictionary<Person, string>()};
            person.Dictionary.Add(new Person{Name = "Pasha"}, "123");
            person.Dictionary.Add(new Person{Name = "Misha"}, "456");
            person.Dictionary.Add(new Person {Name = "Dasha"}, "789");
            person.Dictionary.Add(person, "09876");
            a = new A {X = "xxx", Y = "ay"};
            var b = new B {Y = "by", A = a};
            a.B = b;
        }
        
        [Test]
        public void TuneSubobjectExcluding()
        {
            var config = ObjectPrinter.For<A>()
                .Excluding(a => a.B.Y);
           
            var str = config.PrintToString(a);
            str.Should().Contain("ay");
            str.Should().NotContain("by");
        }
        
        [Test]
        public void PrintingIEnumerable()
        {
            var str = person.PrintToString();
            str.Should().Contain("1");
            str.Should().Contain("2");
            str.Should().Contain("3");
            str.Should().Contain("4");
            str.Should().Contain("5");
        }

        [Test]
        public void PrintingDictionary()
        {
            var str = person.PrintToString();
            str.Should().Contain("Pasha");
            str.Should().Contain("Misha");
            str.Should().Contain("Dasha");
        }

        [Test]
        public void CompilingMethods()
        {
            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа +
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа + 
                .Printing<int>().Using(i => i.ToString("X"))
                //3. Для числовых типов указать культуру +
                .Printing<double>().Using(new CultureInfo("en"))
                //4. Настроить сериализацию конкретного свойства +
                .Printing(p => p.Name).Using(s => s + " 111")
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств) +
                .Printing(p => p.Name).TrimmedToLength(6)
                //6. Исключить из сериализации конкретного свойства + 
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
        public void TypeExcluding()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<Guid>();
            printer.PrintToString(person).Should().NotContain("Guid");
            printer.PrintToString(person).Should().NotContain("Id");
        }
        
        [Test]
        public void PropertyExcluding()
        {
            var printer = ObjectPrinter.For<Person>().Excluding(p => p.Age);
            printer.PrintToString(person).Should().NotContain("Age");
            printer.PrintToString(person).Should().NotContain("19");
        }
        
        [Test]
        public void TypePrintingUsingConfig()
        {
            var printer = ObjectPrinter.For<Person>().Printing<int>().Using(i => i.ToString("X"));
            printer.PrintToString(person).Should().NotContain("19");
            printer.PrintToString(person).Should().Contain("13");
        }
        
        [Test]
        public void TypePrintingUsingCulture()
        {
            var printer = ObjectPrinter.For<Person>().Printing<double>().Using(new CultureInfo("en"));
            printer.PrintToString(person).Should().NotContain("1,85");
            printer.PrintToString(person).Should().Contain("1.85");
        }
        
        [Test]
        public void PropertyPrintingUsingConfig()
        {
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Name).Using(s => s + " 111");
            printer.PrintToString(person).Should().Contain("Alex 111");
        }
        
        [Test]
        public void PropertyStringTrimmedToLength()
        {
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Name).TrimmedToLength(2);
            printer.PrintToString(person).Should().Contain("Al");
            printer.PrintToString(person).Should().NotContain("Alex");
        }
    }
}