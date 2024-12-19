using System.Globalization;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        private static readonly VerifySettings DefaultSettings = new();
        private Person person;
        private static Task Verify(string printedObject) => Verifier.Verify(printedObject, DefaultSettings);

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            DefaultSettings.UseDirectory("ForVerifier");
            person = new Person
            {
                Id = new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3301"),
                Age = 39,
                Name = "Dima",
                Height = 176.0,
                BirthDate = new DateTime(1985, 07, 27)
            };

            var homeworks = new List<string>()
            {
                "Testing",
                "Tag cloud",
                "Markdown",
                "Object Printer"
            };
            person.HomeworksCompleted = homeworks;

            var friends = new[]
            {
                new Person() { Name = "Nikita" },
                new Person() { Name = "Pasha" }
            };
            person.Friends = friends;

            var emergencyCallList = new Dictionary<string, string>()
            {
                {"wife","8-800-5-5-5-3-5-3-5"},
                {"father", "3-10-10-10"},
                {"mother", "03"}
            };
            person.EmergensyList = emergencyCallList;
        }

        [Test]
        public Task Excluding_ShouldExcludePropertyOfType()
        {
            var result = ObjectPrinter.For<Person>()
                .Excluding<string>()
                .PrintToString(person);

            return Verify(result);
        }
        
        [Test]
        public Task Excluding_ShouldExcludeConcreteProperty()
        {
            var result = ObjectPrinter.For<Person>()
                .Excluding(p => p.ToExclude)
                .Excluding(p => p.Id)
                .PrintToString(person);

            return Verify(result);
        }

        [Test]
        public Task Using_ShouldSetPrinterForType()
        {
            var result = ObjectPrinter.For<Person>()
                .Printing<int>().Using(_ => "XX")
                .PrintToString(person);

            return Verify(result);
        }

        [Test]
        public Task Using_ShouldSetPrinterForProperty()
        {
            var result = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).Using(_ => "XX")
                .PrintToString(person);

            return Verify(result);
        }

        [Test]
        public Task SetCulture_ShouldSetCulture()
        {
            var result = ObjectPrinter.For<Person>()
                .Printing<DateTime>().SetCulture(new CultureInfo("en"))
                .PrintToString(person);

            return Verify(result);
        }

        [TestCase(10, TestName = "Print original string when its length smaller than needed") ]
        [TestCase(3, TestName = "Print trimmed string in right way")]
        public Task TrimmedToLength_ShouldTrimStringType(int maxLen)
        {
            var result = ObjectPrinter.For<Person>()
                .Printing<string>().TrimmedToLength(maxLen)
                .PrintToString(person);

            return Verify(result);
        }

        private class Foo
        {
            public Foo Self { get; set; }
        }

        [Test]
        public Task ObjectPrinter_ShouldFindLoop()
        {
            var foo = new Foo();
            foo.Self = foo;

            var result = foo.PrintToString();

            return Verify(result);
        }

        [Test]
        public Task Demo()
        {
            var result = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                 //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(_ => "XX")
                //3. Для типов указать культуру
                .Printing<DateTime>().SetCulture(new CultureInfo("en"))
                //4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Age).Using(_ => "Hidden")
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing<string>().TrimmedToLength(5)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.ToExclude)
                .PrintToString(person);

            return Verify(result);
        }
    }
}