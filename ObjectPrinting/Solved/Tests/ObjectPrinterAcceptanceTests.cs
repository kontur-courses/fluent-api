using System;
using System.Globalization;
using NUnit.Framework;
using FluentAssertions;

namespace ObjectPrinting.Solved.Tests
{
	[TestFixture]
	public class ObjectPrinterAcceptanceTests
	{

		[Test]
		public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19 , Height = 55.5};
            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => "X")
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(new CultureInfo(42))
                //4. Настроить сериализацию конкретного свойства
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(10)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Name);

			string s1 = printer.PrintToString(person);
			
			//7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
			string s2 = person.PrintToString();
			
			//8. ...с конфигурированием
			string s3 = person.PrintToString(s => s.Excluding(p => p.Height).Printing<int>().Using(i => "qwe"));
			Console.WriteLine(s1);
			Console.WriteLine(s2);
			Console.WriteLine(s3);
		}

	    [Test]
	    public void ExcludingTypeTest()
	    {
	        var printer = ObjectPrinter.For<Person>()
	            .Excluding<Guid>();
	        var expectedResult = String.Join(Environment.NewLine,
	            new[]
	            {
	                "Person",
	                "\tName = Alex",
	                "\tHeight = 0",
	                "\tAge = 19" + Environment.NewLine
	            });
            printer.PrintToString(new Person { Name = "Alex", Age = 19 })
                .ShouldBeEquivalentTo(expectedResult);
        }

        [Test]
        public void ExcludingPropertyTest()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Id);
            var expectedResult = String.Join(Environment.NewLine,
                new[]
                {
                    "Person",
                    "\tName = Alex",
                    "\tHeight = 0",
                    "\tAge = 19" + Environment.NewLine
                });
            printer.PrintToString(new Person { Name = "Alex", Age = 19 })
                .ShouldBeEquivalentTo(expectedResult);
        }

        [Test]
        public void TypeSerialisationTest()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<Guid>().Using(g => "WAAAGH");
            var expectedResult = String.Join(Environment.NewLine,
                new[]
                {
                    "Person",
                    "\tId = WAAAGH",
                    "\tName = Alex",
                    "\tHeight = 0",
                    "\tAge = 19" + Environment.NewLine
                });
            printer.PrintToString(new Person { Name = "Alex", Age = 19 })
                .ShouldBeEquivalentTo(expectedResult);
        }

        [Test]
        public void PropertySerialisationTest()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Id).Using(g => "WAAAGH");
            var expectedResult = String.Join(Environment.NewLine,
                new[]
                {
                    "Person",
                    "\tId = WAAAGH",
                    "\tName = Alex",
                    "\tHeight = 0",
                    "\tAge = 19" + Environment.NewLine
                });
            printer.PrintToString(new Person { Name = "Alex", Age = 19 })
                .ShouldBeEquivalentTo(expectedResult);
        }

        [Test]
        public void NumericCultureTest()
        {
            var person = new Person {Name = "Alex", Age = 19, Height = 178.9};
            var culture = new CultureInfo(42);
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(culture);
            var expectedResult = String.Join(Environment.NewLine,
                new[]
                {
                    "Person",
                    "\tId = Guid",
                    "\tName = Alex",
                    "\tHeight = " + person.Height.ToString(culture),
                    "\tAge = 19" + Environment.NewLine
                });
            printer.PrintToString(person)
                .ShouldBeEquivalentTo(expectedResult);
        }

        [Test]
        public void StringTypeTrimmTest()
        {
            var person = new Person { Name = "Artyom Gorodetskiy", Age = 19 };
            var culture = new CultureInfo(42);
            var printer = ObjectPrinter.For<Person>()
                .Printing<string>().TrimmedToLength(10);
            var expectedResult = String.Join(Environment.NewLine,
                new[]
                {
                    "Person",
                    "\tId = Guid",
                    "\tName = " + person.Name.Substring(0, 10),
                    "\tHeight = 0",
                    "\tAge = 19" + Environment.NewLine
                });
            printer.PrintToString(person)
                .ShouldBeEquivalentTo(expectedResult);
        }

        [Test]
        public void StringPropertyTrimmTest()
        {
            var person = new Person { Name = "Artyom Gorodetskiy", Age = 19 };
            var culture = new CultureInfo(42);
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(10);
            var expectedResult = String.Join(Environment.NewLine,
                new[]
                {
                    "Person",
                    "\tId = Guid",
                    "\tName = " + person.Name.Substring(0, 10),
                    "\tHeight = 0",
                    "\tAge = 19" + Environment.NewLine
                });
            printer.PrintToString(person)
                .ShouldBeEquivalentTo(expectedResult);
        }

        [Test]
        public void SerialisationIsMoreImportantThenTrimmTest()
        {
            var person = new Person { Name = "Artyom Gorodetskiy", Age = 19 };
            var culture = new CultureInfo(42);
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(10)
                .Printing(p => p.Name).Using(n => "Artyomka");
            var expectedResult = String.Join(Environment.NewLine,
                new[]
                {
                    "Person",
                    "\tId = Guid",
                    "\tName = Artyomka",
                    "\tHeight = 0",
                    "\tAge = 19" + Environment.NewLine
                });
            printer.PrintToString(person)
                .ShouldBeEquivalentTo(expectedResult);
        }

        [Test]
        public void ObjectExtantionTest()
        {
            var person = new Person { Name = "Artyom Gorodetskiy", Age = 19 };
            var culture = new CultureInfo(42);
            var expectedResult = String.Join(Environment.NewLine,
                new[]
                {
                    "Person",
                    "\tId = Guid",
                    "\tName = Artyom Gorodetskiy",
                    "\tHeight = 0",
                    "\tAge = 19" + Environment.NewLine
                });
            person.PrintToString().ShouldBeEquivalentTo(expectedResult);
        }

        [Test]
        public void ObjectExtantionWithParametersTest()
        {
            var person = new Person { Name = "Artyom Gorodetskiy", Age = 19 };
            var culture = new CultureInfo(42);
            var expectedResult = String.Join(Environment.NewLine,
                new[]
                {
                    "Person",
                    "\tId = Guid",
                    "\tName = Artyomka",
                    "\tHeight = 0",
                    "\tAge = 19" + Environment.NewLine
                });
            person.PrintToString(q => q
                .Printing(p => p.Name).TrimmedToLength(10)
                .Printing(p => p.Name).Using(n => "Artyomka"))
                .ShouldBeEquivalentTo(expectedResult);
        }
    }
}