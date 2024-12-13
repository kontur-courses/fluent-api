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

        [Test]
        public Task ObjectPrinter_ShouldTrimStringType()
        {
            var result = ObjectPrinter.For<Person>()
                .Printing<string>().TrimmedToLength(3)
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
    }
}