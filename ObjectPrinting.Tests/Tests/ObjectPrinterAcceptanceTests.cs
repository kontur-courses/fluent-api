using FluentAssertions;
using FluentAssertions.Primitives;
using NUnit.Framework;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting.Tests.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private Person person;
        private PrintingConfig<Person> personPrinter;

        [SetUp]
        public void SetUp()
        {
            person = new Person() 
            { 
                Id = Guid.NewGuid(),
                Name = "John Doe", 
                Height = 175.5
            };
            personPrinter = ObjectPrinter.For<Person>();
        }

        [Test]
        public void PrintListOfInts_ReturnCorrectString()
        {
            var list = new List<int>() { 1, 2, 3 };
            var printer = ObjectPrinter.For<List<int>>();

            var actual = printer.PrintToString(list);

            Assert.AreEqual("[1,2,3]", actual);
        }

        [Test]
        public void PrintArrayOfInts_ReturnCorrectString()
        {
            var list = new int[] { 1, 2, 3 };
            var printer = ObjectPrinter.For<int[]>();

            var actual = printer.PrintToString(list);

            Assert.AreEqual("[1,2,3]", actual);
        }

        [Test]
        public void PrintListDictOfStrings_ReturnCorrectString()
        {
            var dict = new Dictionary<int, string>
            {
                { 1, "a" },
                { 2, "b" }
            };
            var printer = ObjectPrinter.For<Dictionary<int, string>>();

            var actual = printer.PrintToString(dict);

            Assert.AreEqual("{1 : a,2 : b}", actual);
        }

        [Test]
        public void ExcludeProperty_Success()
        {
            personPrinter.Excluding(o => o.Height);

            var actual = personPrinter.PrintToString(person);

            StringAssert.DoesNotContain("Height", actual);
        }

        [Test]
        public void ExcludeType_Success()
        {
            personPrinter.Excluding<Guid>();

            var actual = personPrinter.PrintToString(person);

            StringAssert.DoesNotContain("Id", actual);
        }

        [Test]
        public void PrintForTypes_Success()
        {
            personPrinter.Print<string>().Using(o => "ABCDEF");

            var actual = personPrinter.PrintToString(person);

            StringAssert.Contains("ABCDEF", actual);
        }

        [Test]
        public void PrintForProperties_Success()
        {
            personPrinter.Print(o => o.Name).Using(o => "ABCDEF");

            var actual = personPrinter.PrintToString(person);

            StringAssert.Contains("ABCDEF", actual);
        }

        [Test]
        public void TruncateString_Success()
        {
            personPrinter.Print<string>().TruncateLength(6);

            var actual = personPrinter.PrintToString(person);

            StringAssert.DoesNotContain("John Do", actual);
            StringAssert.Contains("John D", actual);
        }

        [Test]
        public void SetMaxStringLengthViaConfigure_Success()
        {
            personPrinter.Configure(opt => opt.MaxStringLength = 6);

            var actual = personPrinter.PrintToString(person);

            StringAssert.DoesNotContain("John Do", actual);
            StringAssert.Contains("John D", actual);
        }

        [TestCase("ru-RU")]
        [TestCase("en-US")]
        [TestCase("fr-FR")]
        public void SetCulture_ShouldApplyCulture(string code)
        {
            var culture = new CultureInfo(code);
            personPrinter.Print<double>()
                         .SetCulture(culture);
            var should = person.Height.ToString(culture);


            var actual = personPrinter.PrintToString(person);


            StringAssert.Contains(should, actual);
        }

        [TestCase("ru-RU")]
        [TestCase("en-US")]
        [TestCase("fr-FR")]
        public void SetCultureViaConfigure_ShouldApplyCulture(string code)
        {
            var culture = new CultureInfo(code);
            personPrinter.Configure(opt => opt.CultureInfo = culture);
            var should = person.Height.ToString(culture);


            var actual = personPrinter.PrintToString(person);


            StringAssert.Contains(should, actual);
        }

        [Test]
        public void CyclicReference_DoesnThrowStackOverflow()
        {
            var node1 = new Node() { Value = 1 };
            var node2 = new Node() { Value = 2 };
            node1.Next = node2;
            node2.Next = node1;
            var printer = ObjectPrinter.For<Node>();

            Console.WriteLine(printer.PrintToString(node1));
            Assert.DoesNotThrow(() => printer.PrintToString(node1));
        }

        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 172.1,
                Father = new Parent() { Name = "Bob Robinson", Age = 54, Height = 182.2},
                Mother = new Parent() { Name = "Sara Robinson", Age = 50, Height = 185.4}
            };

            var printer = ObjectPrinter.For<Person>();
            //1. Исключить из сериализации свойства определенного типа
            //2. Указать альтернативный способ сериализации для определенного типа
            //3. Для числовых типов указать культуру
            //4. Настроить сериализацию конкретного свойства
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            //6. Исключить из сериализации конкретного свойства

            printer.Excluding(o => o.Id)
                   .Print(o => o.Height)
                   .Using(h => $"{h} сантиметров")
                   .Print(o => o.Name)
                   .TruncateLength(10)
                   .Configure(opts =>
                   {
                       opts.MaxStringLength = 100;
                       opts.CultureInfo = CultureInfo.InvariantCulture;
                   });
                   
                                                  

            string s1 = printer.PrintToString(person);

            Console.WriteLine(s1);

            s1.Should().Be(s1);
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }    
    }
}