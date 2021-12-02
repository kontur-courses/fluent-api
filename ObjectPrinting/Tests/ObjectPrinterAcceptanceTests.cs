using NUnit.Framework;
using System.Globalization;
using FluentAssertions;
using System;
using System.Linq.Expressions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Exclude<string>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString())
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).CropToLength(1)
                //6. Исключить из сериализации конкретного свойства
                .Exclude(p => p.Age);

            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }

        [Test]
        public void Exclude_OnPerson_ShouldExclude()
        {
            var expected = "Person\r\n\tId = Guid\r\n\tHeight = 0\r\n\tAge = 19\r\n";
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>().Exclude<string>();
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void DifferentSerializationForType_OnPerson_ShouldWork()
        {
            var expected = "Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = 190\r\n";
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>().Printing<int>().Using(x => (((int)x)*10).ToString());
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void DifferentSerializationForProperty_OnPerson_ShouldWork()
        {
            var expected = "Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = 20\r\n";
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Age).Using(x => (((int)x) + 1).ToString());
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void DifferentSerializationForStringProperty_OnPerson_ShouldWork()
        {
            var expected = "Person\r\n\tId = Guid\r\n\tName = Al\r\n\tHeight = 0\r\n\tAge = 19\r\n";
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Name).CropToLength(2);
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ExcludingProperty_OnPerson_ShouldWork()
        {
            var expected = "Person\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = 19\r\n";
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>().Exclude(p => p.Id);
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void AllPossibleParameters_OnPerson_ShouldWork()
        {
            var expected = "Person\r\n\tName = Ale\r\n\tHeight = 100,10223\r\n";
            var person = new Person { Name = "Alex", Age = 10, Height = 100.10223 };
            var printer = ObjectPrinter
                .For<Person>()
                .Exclude(p => p.Id)
                .Printing(p => p.Name)
                .CropToLength(3)
                .Printing<Guid>()
                .Using(x => x.ToString() + " нули хехе")
                .Printing<double>()
                .Using(CultureInfo.InstalledUICulture)
                .Printing<int>()
                .Using(x => ((int)x + 5).ToString())
                .Exclude<int>();

            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void Double_OnRussianCulture_ShouldBeRussian()
        {
            var num = 1.2d;
            var printer = ObjectPrinter.For<double>()
                .Printing<double>().Using(CultureInfo.GetCultureInfoByIetfLanguageTag("ru-ru"));

            printer.PrintToString(num).Should().Be("1,2\r\n");
        }

        [Test]
        public void Double_OnAmericanCulture_ShouldBeAmerican()
        {
            var num = 1.2d;
            var printer = ObjectPrinter.For<double>()
                .Printing<double>().Using(CultureInfo.GetCultureInfoByIetfLanguageTag("en-us"));

            printer.PrintToString(num).Should().Be("1.2\r\n");
        }

        [Test]
        public void PrintToString_OnNodeWithNoOtherNodesAndNoConfigs_ShouldWorkCorrect()
        {
            var node = new Node();

            var printer = ObjectPrinter.For<Node>();
            var actual = printer.PrintToString(node);
            actual.Should().Be("Node\r\n\tOtherNode = null\r\n\tValue = 0\r\n");
        }

        [Test]
        public void PrintToString_OnObjWithCyclicReference_ShouldHandleIt()
        {
            var node = new Node(2);
            node.AddNode(new Node(3));

            var printer = ObjectPrinter.For<Node>();
            var actual = printer.PrintToString(node);
            actual.Should().Be("Node\r\n\tOtherNode = Node\r\n\t\tOtherNode = Cyclic reference found.\r\n\t\tValue = 3\r\n\tValue = 2\r\n");
        }

        [Test]
        public void PrintToString_OnObjWithList_ShouldWorkCorrect()
        {
            var obj = new ClassWithList();
            obj.Values.Add(100);
            obj.Values.Add(200);
            obj.Values.Add(300);

            var expected = "ClassWithList\r\n\tValues = List`1\r\n\t\tCapacity = 4\r\n\t\tCount = 3\r\n\t\tElements:\r\n\t\t\t100\r\n\t\t\t200\r\n\t\t\t300\r\n\tValue = 0\r\n";
            var printer = ObjectPrinter.For<ClassWithList>();
            var actual = printer.PrintToString(obj);
            actual.Should().Be(expected);
        }

        [Test]
        public void PrintToString_TypeAndPropertyConflictInClass_ShouldGoForProperty()
        {
            var person = new Person() { Age = 1, Name = "Alex"};

            var printer = ObjectPrinter.For<Person>()
                .Printing<int>()
                .Using(x => (1 + ((int)x)).ToString())
                .Printing(p => p.Age)
                .Using(x => (2 + ((int)x)).ToString())
                .Exclude(p => p.Height)
                .Exclude(p => p.Id)
                .Exclude(p => p.Name);

            var exp = "Person\r\n\tAge = 3\r\n";

            printer.PrintToString(person).Should().Be(exp);
        }
    }
}