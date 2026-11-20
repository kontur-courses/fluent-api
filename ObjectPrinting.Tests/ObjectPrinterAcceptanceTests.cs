using FluentAssertions;
using FluentAssertions.Execution;
using ObjectPrinting.Tests.Models;
using System.Globalization;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        public Person MakeTestPerson()
        {
            return new Person() { Id = Guid.NewGuid(), Age = 20, Name = "Ruslan", Height = 170.12 };
        }

        public RecursivePerson MakeTestRecursivePerson()
        {
            var person1 = new RecursivePerson() { Id = Guid.NewGuid(), Age = 20, Name = "Ruslan", Height = 170.12 };
            var person2 = new RecursivePerson() { Age = 25, Name = "Данил", Height = 175, Id = Guid.Empty, Children = person1 };
            person1.Children = person2;
            return person2;
        }

        public Shop MakeTestShop()
        {
            return new Shop() { PersonalNames = new string[] { "Данил", "Руслан", "Максимка" },
                Products = new List<Product>()
                {
                    new Product() { ProductName = "Кашка" },
                    new Product() { ProductName = "Греча" },
                    new Product() { ProductName = "Супец" },
                },
                PersonalSalary = new Dictionary<string, int>()
                {
                    { "Данил", 100000 },
                    { "Руслан", 0 },
                    { "Максимка", -100 }
                }
            };
        }

        [Test]
        public void ObjectPrinter_TestSyntax()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства/поля определенного типа
                .Excluding<Guid>()
                //2. Альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => (i + 99).ToString())
                //3. Для всех типов, имеющих культуру, есть возможность ее указать
                .Printing<double>().UsingCulture(CultureInfo.InvariantCulture)
                //4. Настройка сериализации конкретного свойства/поля
                .Printing(p => p.Age).Using(i => (i - 99).ToString())
                //5. Возможность обрезания строк (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(10)
                //6. Исключение из сериализации конкретного свойства/поля
                .Excluding(p => p.Age)
                // 7. Корректная обработка циклических ссылок между объектами (не должны приводить к StackOverflowException)
                .WithRecursionLimit(2);

            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            string s2 = person.PrintToString();

            //8. ...с конфигурированием
            string s3 = person.PrintToString(s => s.Excluding(p => p.Age));
            Assert.Pass();
        }

        [Test]
        public void ObjectPrinter_TestExcludingType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<string>();
            var result = printer.PrintToString(MakeTestPerson());
            result.Should().Contain("Id = ");
            result.Should().Contain("Height = ");
            result.Should().Contain("Age = ");

            result.Should().NotContain("Name = ");
        }

        [Test]
        public void ObjectPrinter_TestExceptionInExcludingType()
        {
            var act = () => ObjectPrinter.For<Person>().Excluding<Person>();
            act.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void ObjectPrinter_TestPrintingUsingForTypes()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<string>().Using(str => str.ToUpper() + "11225");
            var result = printer.PrintToString(MakeTestPerson());
            result.Should().Contain("Id = ");
            result.Should().Contain("Height = ");
            result.Should().Contain("Age = ");
            result.Should().Contain("Name = RUSLAN11225");
        }

        [Test]
        public void ObjectPrinter_TestPrintingUsingCulture()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().UsingCulture(CultureInfo.InvariantCulture);
            var result = printer.PrintToString(MakeTestPerson());
            result.Should().Contain("Id = ");
            result.Should().Contain("Height = 170.12");
            result.Should().Contain("Age = ");
            result.Should().Contain("Name = ");
        }

        [Test]
        public void ObjectPrinter_TestExceptionInPrintingUsingCulture()
        {
            Action act = () => ObjectPrinter.For<Person>()
                .Printing(p => p.Height).UsingCulture(CultureInfo.InvariantCulture);

            act.Should().Throw<InvalidOperationException>();

        }

        [Test]
        public void ObjectPrinter_TestPrintingUsingForFields()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Age).Using(age => (age + 1000).ToString());
            var result = printer.PrintToString(MakeTestPerson());
            result.Should().Contain("Id = ");
            result.Should().Contain("Height = ");
            result.Should().Contain("Age = 1020");
            result.Should().Contain("Name = ");
        }

        [Test]
        public void ObjectPrinter_TestThatUsingForFieldsHigherThenForTypes()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Age).Using(age => (age + 1000).ToString())
                .Printing<int>().Using(num => (num - 1000).ToString());
            var result = printer.PrintToString(MakeTestPerson());
            result.Should().Contain("Id = ");
            result.Should().Contain("Height = ");
            result.Should().Contain("Age = 1020");
            result.Should().Contain("Name = ");
        }

        [Test]
        public void ObjectPrinter_TestTrimmingStrings()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(3);
            var result = printer.PrintToString(MakeTestPerson());
            result.Should().Contain("Id = ");
            result.Should().Contain("Height = ");
            result.Should().Contain("Age = ");
            result.Should().Contain("Name = Rus");
            result.Should().NotContain("Name = Ruslan");
        }

        [Test]
        public void ObjectPrinter_TestExceptionInTrimmingStrings()
        {
            var act = () => ObjectPrinter.For<Person>()
                .Printing<string>().TrimmedToLength(3);

            act.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void ObjectPrinter_TestExcludingField()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Age);
            var result = printer.PrintToString(MakeTestPerson());
            result.Should().Contain("Id = ");
            result.Should().Contain("Height = ");
            result.Should().NotContain("Age = ");
            result.Should().Contain("Name = ");
        }

        [Test]
        public void ObjectPrinter_TestRecursionLimit()
        {
            var printer = ObjectPrinter.For<RecursivePerson>()
                .WithRecursionLimit(2);
            var result = printer.PrintToString(MakeTestRecursivePerson());
            result.Split("Данил").Length.Should().Be(3);
            result.Split("Ruslan").Length.Should().Be(3);
            result.Split("Name = ").Length.Should().Be(5);
        }

        [Test]
        public void ObjectPrinter_TestExceptionInRecursionLimit()
        {
            var act = () => ObjectPrinter.For<RecursivePerson>()
                .WithRecursionLimit(-1);
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void ObjectPrinter_TestPrintArray()
        {
            var printer = ObjectPrinter.For<Shop>()
                .Excluding(shop => shop.Products)
                .Excluding(shop => shop.PersonalSalary);
            var result = printer.PrintToString(MakeTestShop()).Replace("\t", "    ");
            result.Should().Be(File.ReadAllText(Path.Combine("TestResults", "array_test_result.txt")));
        }

        [Test]
        public void ObjectPrinter_TestPrintList()
        {
            var printer = ObjectPrinter.For<Shop>()
                .Excluding(shop => shop.PersonalNames)
                .Excluding(shop => shop.PersonalSalary);
            var result = printer.PrintToString(MakeTestShop()).Replace("\t", "    ");
            result.Should().Be(File.ReadAllText(Path.Combine("TestResults", "list_test_result.txt")));
        }

        [Test]
        public void ObjectPrinter_TestPrintDictionary()
        {
            var printer = ObjectPrinter.For<Shop>()
                .Excluding(shop => shop.PersonalNames)
                .Excluding(shop => shop.Products);
            var result = printer.PrintToString(MakeTestShop()).Replace("\t", "    ");
            result.Should().Be(File.ReadAllText(Path.Combine("TestResults", "dictionary_test_result.txt")));
        }

        [Test]
        public void ObjectPrinter_GeneralTest()
        {
            var person = new Person() { Age = 20, Height = 170, Id = Guid.Empty, Name = "Ruslan" };
            person.PrintToString().Should().Be((
                "Person {{Environment.NewLine}" +
                "\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}" +
                "\tName = Ruslan{Environment.NewLine}" +
                "\tHeight = 170{Environment.NewLine}" +
                "\tAge = 20{Environment.NewLine}" +
                "}{Environment.NewLine}").Replace("{Environment.NewLine}", Environment.NewLine));
        }
    }
}