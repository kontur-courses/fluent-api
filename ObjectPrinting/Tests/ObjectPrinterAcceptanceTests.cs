using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    public class A
    {
        public string X { get; set; }
        public B B { get; set; }
    }

    public class B
    {
        public string Y { get; set; }
        public string Z { get; set; }
    }
    
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private Person person = new Person { Name = "Alex", Age = 19, Height = 1.85};
        [Test]
        public void TuneSubobject()
        {
            var config = ObjectPrinter.For<A>()
                .Excluding(a => a.B.Y); // should work
            var obj = new A {X = "xxx", B = new B {Y = "yyy", Z = "zzz"}};

            var str = config.PrintToString(obj);

            Console.WriteLine(str);
        }
        
        [Test]
        public void Demo()
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