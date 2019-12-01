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

        [Test]
        public void TuneSubobjectExcluding()
        {
            var car = new Car()
            {
                Engine = new Engine()
                {
                    Horsepower = 123,
                    Id = 979523
                },
                Id = 8750532,
                Model = "volkswagen"
            };
            var config = ObjectPrinter.For<Car>()
                .Excluding(a => a.Engine.Id);

            var str = config.PrintToString();
            str.Should().NotContain("8750532");
        }
        
        [Test]
        public void PrintingIEnumerable()
        {
            IEnumerable<int> enumerable = new[] {1, 2, 3, 4, 5};

            var str = enumerable.PrintToString();
            str.Should().Contain("1");
            str.Should().Contain("2");
            str.Should().Contain("3");
            str.Should().Contain("4");
            str.Should().Contain("5");
        }

        [Test]
        public void PrintingDictionary()
        {
            var dict = new Dictionary<int, string>
            {
                {1, "Pasha"},
                {22, "Misha"},
                {333, "Dasha"}
            };
            var str = dict.PrintToString();
            str.Should().Contain("1");
            str.Should().Contain("22");
            str.Should().Contain("333");
            str.Should().Contain("Pasha");
            str.Should().Contain("Misha");
            str.Should().Contain("Dasha");
        }

        [Test]
        public void CompilingMethods()
        {
            var person = new Person {Name = "Alex", Age = 19, Height = 1.85};
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
            var person = new Person {Name = "Alex", Age = 19, Height = 1.85};
            var printer = ObjectPrinter.For<Person>().Excluding<Guid>();
            printer.PrintToString(person).Should().NotContain("Guid");
            printer.PrintToString(person).Should().NotContain("Id");
        }
        
        [Test]
        public void PropertyExcluding()
        {
            var person = new Person {Name = "Alex", Age = 19, Height = 1.85};
            var printer = ObjectPrinter.For<Person>().Excluding(p => p.Age);
            printer.PrintToString(person).Should().NotContain("Age");
            printer.PrintToString(person).Should().NotContain("19");
        }
        
        [Test]
        public void FieldExcluding()
        {
            var car = new Car()
            {
                Engine = new Engine()
                {
                    Horsepower = 123,
                    Id = 979523
                },
                Id = 8750532,
                Model = "volkswagen"
            };
            var printer = ObjectPrinter.For<Car>().Excluding(p => p.Model);
            printer.PrintToString(car).Should().NotContain("volkswagen");
        }

        [Test]
        public void TypePrintingUsingConfig()
        {
            var person = new Person {Name = "Alex", Age = 19, Height = 1.85};
            var printer = ObjectPrinter.For<Person>().Printing<int>().Using(i => i.ToString("X"));
            printer.PrintToString(person).Should().NotContain("19");
            printer.PrintToString(person).Should().Contain("13");
        }
        
        [Test]
        public void TypePrintingUsingCulture()
        {
            var person = new Person {Name = "Alex", Age = 19, Height = 1.85};
            var printer = ObjectPrinter.For<Person>().Printing<double>().Using(new CultureInfo("en"));
            printer.PrintToString(person).Should().NotContain("1,85");
            printer.PrintToString(person).Should().Contain("1.85");
        }
        
        [Test]
        public void PropertyPrintingUsingConfig()
        {
            var person = new Person {Name = "Alex", Age = 19, Height = 1.85};
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Name).Using(s => s + " 111");
            printer.PrintToString(person).Should().Contain("Alex 111");
        }
        
        [Test]
        public void FieldPrintingUsingConfig()
        {
            var car = new Car()
            {
                Engine = new Engine()
                {
                    Horsepower = 123,
                    Id = 979523
                },
                Id = 8750532,
                Model = "volkswagen"
            };
            var printer = ObjectPrinter.For<Car>().Printing(p => p.Model).Using(s => s + "997");
            printer.PrintToString(car).Should().Contain("volkswagen997");
        }
        [Test]
        public void PropertyStringTrimmedToLength()
        {
            var person = new Person {Name = "Alex", Age = 19, Height = 1.85};
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Name).TrimmedToLength(2);
            printer.PrintToString(person).Should().Contain("Al");
            printer.PrintToString(person).Should().NotContain("Alex");
        }

        [Test]
        public void WhenFieldNestingMore30_StackOverflowException()
        {
            var vertex = new Vertex();
            var vertex1 = new Vertex();
            vertex.Vertices.Add(vertex1);
            for (var i = 0; i < 30; i++)
            {
                var vertex2 = new Vertex();
                vertex1.Vertices.Add(vertex2);
                vertex1 = vertex2;
            }

            Action action = () => vertex.PrintToString();
            action.Should().Throw<StackOverflowException>();
        }

        [Test]
        public void PrintingCircularReference()
        {
            var vertex = new Vertex();
            vertex.Vertices.Add(vertex);
            vertex.PrintToString().Should().NotBeEmpty();
        }
        
        [Test]
        public void FieldStringTrimmedToLength()
        {
            var car = new Car()
            {
                Engine = new Engine()
                {
                    Horsepower = 123,
                    Id = 979523
                },
                Id = 8750532,
                Model = "volkswagen"
            };
            var printer = ObjectPrinter.For<Car>().Printing(p => p.Model).TrimmedToLength(4);
            printer.PrintToString(car).Should().Contain("volk");
            printer.PrintToString(car).Should().NotContain("volkswagen");
        }
    }
}