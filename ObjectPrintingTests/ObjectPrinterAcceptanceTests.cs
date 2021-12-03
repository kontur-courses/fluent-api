using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using ObjectPrinting;
using FluentAssertions;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private PrintingConfig<Person> printer;
        private Person person;

        [SetUp]
        public void SetUp()
        {
            printer = ObjectPrinter.For<Person>();
            person = new Person
            {
                Name = "Albert",
                Age = 54,
                Height = 146,
                Id = new Guid()
            };
        }


        [Test]
        public void Demo()
        {
            var demoPerson = new Person {Name = "Alex", Age = 19};

            var demoPrinter = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<int>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<double>().Using(x => $"{x + 1}")
                //3. Для числовых типов указать культуру
                .Printing<Guid>().Using(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(x => x.Height).Using(x => $"{x + 1}")
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(x => x.Name).TrimmedToLength(1)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Age);
            string s1 = demoPrinter.PrintToString(demoPerson);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }

        [Test]
        public void Excluding_ShouldExcludeSpecificTypeMembers()
        {
            var trianglePrinter = ObjectPrinter.For<Triangle>();
            var triangle = new Triangle();
            trianglePrinter
                .Excluding<double>()
                .PrintToString(triangle)
                .Should()
                .NotContain("B")
                .And.NotContain("C")
                .And.Contain("A = 1");
        }

        [Test]
        public void Printing_Using_ShouldReplaceSpecialTypeSerialization()
        {
            printer
                .Printing<int>().Using(x => $"{x + 1}")
                .PrintToString(person)
                .Should()
                .Contain($"Age = {person.Age + 1}");
        }

        [Test]
        public void Printing_Using_ShouldSpecifyNewCulture()
        {
            // не придумал проверки  :)
            printer
                .Printing<double>().Using(CultureInfo.GetCultureInfo("ru-Ru"))
                .PrintToString(person)
                .Should()
                .Contain(
                    $"\tHeight = {person.Height.ToString(CultureInfo.GetCultureInfo("ru-Ru"))}{Environment.NewLine}"
                );
        }

        [Test]
        public void Printing_Using_ShouldReplaceSpecialPropertySerialization()
        {
            var trianglePrinter = ObjectPrinter.For<Triangle>();
            var triangle = new Triangle();
            trianglePrinter
                .Printing(r => r.A).Using(x => "123")
                .PrintToString(triangle)
                .Should()
                .Contain("A = 123")
                .And.Contain("B = 1")
                .And.Contain("C = 1");
        }
        
        [Test]
        public void Printing_Using_ShouldReplaceSpecialFieldSerialization()
        {
            var rectPrinter = ObjectPrinter.For<Rectangle>();
            var rect = new Rectangle();
            rectPrinter
                .Printing(r => r.Height).Using(x => "123")
                .PrintToString(rect)
                .Should()
                .Contain("Height = 123")
                .And.Contain("Width = 4");
        }

        [TestCase("Present", 1)]
        [TestCase("Past", 3)]
        [TestCase("Future", 5)]
        public void Printing_TrimmedToLength_ShouldCutStrings(string name, int length)
        {
            var title = new Title(name);
            var titlePrinter = ObjectPrinter.For<Title>();
            titlePrinter
                .Printing(t => t.Name)
                .TrimmedToLength(length)
                .PrintToString(title)
                .Should()
                .Contain(name.Substring(0, length))
                .And.NotContain(name);
        }

        [Test]
        public void Excluding_ShouldExcludeSpecificMembers()
        {
            printer
                .Excluding(p => p.Age)
                .PrintToString(person)
                .Should()
                .NotContain("Age")
                .And.NotContain($"{person.Age}");
        }

        [Test]
        public void PrintToString_ShouldWorkCorrectWithCyclicalLinks_WithoutStackOverflowException()
        {
            var robot = new Robot("Cycle");
            var robotPrinter = ObjectPrinter.For<Robot>();
            Action act = () => robotPrinter.PrintToString(robot);
            act.Should().NotThrow<StackOverflowException>();
        }

        [Test]
        public void PrintToString_ShouldWorkCorrectWithCyclicalLinks_CorrectPrint()
        {
            var robot = new Robot("Oleg");
            var robotPrinter = ObjectPrinter.For<Robot>();
            robotPrinter
                .PrintToString(robot)
                .Should()
                .Contain("Name = Oleg")
                .And.Contain("ItName = Oleg");
        }
        
        [Test]
        public void PrintToString_ShouldSerialize_Array()
        {
            var nums = new []{1, 2, 3};
            var arrayPrinter = ObjectPrinter.For<int[]>();
            arrayPrinter
                .PrintToString(nums)
                .Should()
                .Contain("1")
                .And.Contain("2")
                .And.Contain("3");
        }
        
        [Test]
        public void PrintToString_ShouldSerialize_ObjectArray()
        {
            var robots = new []
            {
                new Robot("r1"),
                new Robot("r2"),
                new Robot("r3") 
            };
            var arrayPrinter = ObjectPrinter.For<Robot[]>();
            arrayPrinter
                .PrintToString(robots)
                .Should()
                .Contain("Robot")
                .And.Contain("Name = r1")
                .And.Contain("Name = r2")
                .And.Contain("Name = r3");
        }
        
        [Test]
        public void PrintToString_ShouldSerialize_List()
        {
            var strList = new List<string>{"ab", "o", "ba"};
            var listPrinter = ObjectPrinter.For<List<string>>();
            listPrinter
                .PrintToString(strList)
                .Should()
                .Contain("ab")
                .And.Contain("o")
                .And.Contain("ba");
        }
        
        [Test]
        public void PrintToString_ShouldSerialize_ObjectList()
        {
            var robots = new List<Robot>
            {
                new Robot("r1"),
                new Robot("r2"),
                new Robot("r3") 
            };
            var arrayPrinter = ObjectPrinter.For<List<Robot>>();
            arrayPrinter
                .PrintToString(robots)
                .Should()
                .Contain("Robot")
                .And.Contain("Name = r1")
                .And.Contain("Name = r2")
                .And.Contain("Name = r3");
        }
        
        [Test]
        public void PrintToString_ShouldSerialize_Dictionary()
        {
            var dict = new Dictionary<string, string>
            {
                {"a", "b"},
                {"b", "c"},
            };
            var listPrinter = ObjectPrinter.For<Dictionary<string, string>>();
            listPrinter
                .PrintToString(dict)
                .Should()
                .Contain("a")
                .And.Contain("b")
                .And.Contain("c");
        }
        
        [Test]
        public void PrintToString_ShouldSerialize_ObjectDictionary()
        {
            var robots = new Dictionary<string, Robot>
            {
                {"a", new Robot("r1") },
                {"b", new Robot("r2") }
            };
            var arrayPrinter = ObjectPrinter.For<Dictionary<string, Robot>>();
            arrayPrinter
                .PrintToString(robots)
                .Should()
                .Contain("Robot")
                .And.Contain("Name = r1")
                .And.Contain("Name = r2")
                .And.Contain("a")
                .And.Contain("b");
        }
    }
}