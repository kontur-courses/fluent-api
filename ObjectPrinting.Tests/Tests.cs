using System.Globalization;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class Tests
    {
        private static readonly VerifySettings DefaultSettings = new();
        private Person person;

        [SetUp]
        public void Setup()
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
        }

        [Test]
        public Task ObjectPrinter_ShouldExcludePropertyOfType()
        {
            var result = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .PrintToString(person);
            
            return Verifier.Verify(result, DefaultSettings);
        }

        [Test]
        public Task ObjectPrinter_ShouldSetPrinterForType()
        {
            var result = ObjectPrinter.For<Person>()
                .Printing<int>().Using(i => "XX")
                .PrintToString(person);
            
            return Verifier.Verify(result, DefaultSettings);
        }

        [Test]
        public Task ObjectPrinter_ShouldSetPrinterForProperty()
        {
            var result = ObjectPrinter.For<Person>()
                .Printing<string>(p => p.Name).Using(i => "XX")
                .PrintToString(person);

            return Verifier.Verify(result, DefaultSettings);
        }

        [Test]
        public Task ObjectPrinter_ShouldSetCulture()
        {
            var result = ObjectPrinter.For<Person>()
                .Printing<DateTime>().SetCulture(new CultureInfo("en"))
                .PrintToString(person);

            return Verifier.Verify(result, DefaultSettings);
        }

        [TestCase(10, TestName = "Print original string when its length smaller than needed") ]
        [TestCase(3, TestName = "Print trimmed string in right way")]
        public Task ObjectPrinter_ShouldTrimStringType(int maxLen)
        {
            var result = ObjectPrinter.For<Person>()
                .Printing<string>().TrimmedToLength(maxLen)
                .PrintToString(person);

            return Verifier.Verify(result, DefaultSettings);
        }

        [Test]
        public Task ObjectPrinter_ShouldFindLoop()
        {
            var spouse = new Person()
            {
                Id = new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3302"),
                Age = 35,
                Name = "Alla",
                Height = 170.0,
                BirthDate = new DateTime(1989, 2, 19),
                Husband = person
            };
            person.Wife = spouse;

            var result = ObjectPrinter.For<Person>()
                .PrintToString(person);

            return Verifier.Verify(result, DefaultSettings);
        }

        [Test]
        public Task ObjectPrinter_ShouldPrintList()
        {
            var homeworks = new List<string>()
            {
                "Testing",
                "Tag cloud",
                "Markdown",
                "Object Printer"
            };
            person.HomeworksCompleted = homeworks;

            var result = ObjectPrinter.For<Person>()
                .PrintToString(person);

            return Verifier.Verify(result, DefaultSettings);
        }

        [Test]
        public Task ObjectPrinter_ShouldPrintArray()
        {
            var friends = new[]
            {
                new Person() { Name = "Nikita" },
                new Person() { Name = "Pasha" }
            };
            person.Friends = friends;

            var result = ObjectPrinter.For<Person>()
                .PrintToString(person);

            return Verifier.Verify(result, DefaultSettings);
        }

        [Test]
        public Task ObjectPrinter_ShouldPrintDictionary()
        {
            var emergencyCallList = new Dictionary<string, string>()
            {
                {"wife","5-5-5-3-5-3-5"},
                {"father", "8-800-2000-600"},
                {"mother", "+7 922 22 222 22"}
            };
            person.EmergensyList = emergencyCallList;

            var result = ObjectPrinter.For<Person>()
                .PrintToString(person);

            return Verifier.Verify(result, DefaultSettings);
        }
    }
}