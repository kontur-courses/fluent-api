using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using System;
using System.Globalization;
using System.Linq;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private Person testObject;
        private string result;

        [SetUp]
        public void CreateTestingObject()
        {
            testObject = new Person
            {
                Name = "Alex",
                Age = 19,
                Height = 180.3,
                Id = Guid.NewGuid(),
                OtherPersons = new[] { Guid.NewGuid(), Guid.Empty, Guid.NewGuid() },
                Biography = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. 
Praesent et erat maximus, dapibus velit a, placerat ligula. In at mauris et ante tristique ornare eget eu dui. 
Curabitur ornare ligula eu tincidunt eleifend. 
Integer convallis, eros ut iaculis fringilla, ligula neque varius urna, a blandit est nulla vel purus.
Vivamus mollis ante et condimentum tempus. Praesent iaculis elit sed bibendum varius. 
Morbi elementum turpis turpis, sed sollicitudin mi sagittis non. Quisque id facilisis tortor. 
Ut dapibus neque non nisl condimentum efficitur. Etiam id libero eget purus rhoncus ultricies. 
Integer nisl quam, bibendum in eleifend vitae, tempus ac massa. Donec mollis.",
                Dates = new()
                {
                    [DateTime.Now] = "today",
                    [DateTime.Now - TimeSpan.FromDays(10)] = "10 days ago"
                }
            };
        }

        [TearDown]
        public void ResultToOutput()
        {
            TestContext.WriteLine(result);
        }

        [Test]
        [MaxTime(1000)]
        public void Should_HandleCyclicLinks()
        {
            var parent = new Person
            {
                Name = "Alex Parent",
                Id = Guid.NewGuid(),
                Age = 40,
                Child = testObject
            };
            testObject.Parent = parent;

            var printer = ObjectPrinter<Person>.Default;

            result = printer.PrintToString(testObject);

            Assert.Pass();
        }

        [Test]
        public void Should_PrintDictionaries()
        {
            testObject.Name = "Really long Name";

            result = ObjectPrinter<Person>.Print(testObject);

            result.Should().ContainAll(testObject.Dates.Select(x => x.Key.ToString()))
                .And.ContainAll(testObject.Dates.Select(x => x.Value.ToString()));
        }

        [Test]
        public void Should_PrintCollections()
        {
            testObject.Name = "Really long Name";

            result = ObjectPrinter<Person>.Configure(options => options.WithSerializer<Guid>(guid => guid.ToString())).PrintToString(testObject);

            result.Should().ContainAll(testObject.OtherPersons.Select(x => x.ToString()));
        }

        [Test]
        public void Should_PrintSpecificTrimmedStrings_ByOptions()
        {
            testObject.Name = "Really long Name";
            var printer = ObjectPrinter<Person>.Configure(options => options.ForProperty(x => x.Name, x => x.WithTrimLength(10)));

            result = printer.PrintToString(testObject);

            result.Should().Contain(testObject.Name[..10]).And.NotContain(testObject.Name)
                .And.Contain(testObject.Biography);
        }

        [Test]
        public void Should_PrintSpecificTrimmedStrings()
        {
            testObject.Name = "Really long Name";
            var printer = ObjectPrinter<Person>.Configure(options => options.WithTrimLength(x => x.Name, 10));

            result = printer.PrintToString(testObject);

            result.Should().Contain(testObject.Name[..10]).And.NotContain(testObject.Name)
                .And.Contain(testObject.Biography);
        }

        [Test]
        public void Should_PrintTrimmedStrings_ByOptions()
        {
            var printer = ObjectPrinter<Person>.Configure(options => options.ForProperties<string>(x => x.WithTrimLength(10)));

            result = printer.PrintToString(testObject);

            result.Should().Contain(testObject.Biography[..10]).And.NotContain(testObject.Biography);
        }

        [Test]
        public void Should_PrintTrimmedStrings()
        {
            var printer = ObjectPrinter<Person>.Configure(options => options.WithTrimLength(10));

            result = printer.PrintToString(testObject);

            result.Should().Contain(testObject.Biography[..10]).And.NotContain(testObject.Biography);
        }

        [Test]
        public void Should_PrintUsingTypeSpecificCulture_ByOptions()
        {
            var printer = ObjectPrinter<Person>
                .Configure(options => options.ForProperties<double>(x => x.WithCulture(CultureInfo.InvariantCulture)));

            result = printer.PrintToString(testObject);

            result.Should().Contain(testObject.Height.ToString(null, CultureInfo.InvariantCulture));
        }

        [Test]
        public void Should_PrintUsingTypeSpecificCulture()
        {
            var printer = ObjectPrinter<Person>.Configure(options => options.WithCulture<double>(CultureInfo.InvariantCulture));

            result = printer.PrintToString(testObject);

            result.Should().Contain(testObject.Height.ToString(null, CultureInfo.InvariantCulture));
        }

        [Test]
        public void Should_PrintUsingTypeSpecificSerializer_ByOptions()
        {
            var printer = ObjectPrinter<Person>.Configure(options => options
                .ForProperties<Guid>(x => x.WithSerializer(y => y.ToString())));

            result = printer.PrintToString(testObject);

            result.Should().Contain(testObject.Id.ToString());
        }

        [Test]
        public void Should_PrintUsingTypeSpecificSerializer()
        {
            var printer = ObjectPrinter<Person>.Configure(options => options.WithSerializer<Guid>(y => y.ToString()));

            result = printer.PrintToString(testObject);

            result.Should().Contain(testObject.Id.ToString());
        }

        [Test]
        public void ShouldNot_PrintExcludedTypes()
        {
            var printer = ObjectPrinter<Person>.Configure(options => options.Exclude<Guid>());

            result = printer.PrintToString(testObject);

            result.Should().NotContain(testObject.Id.ToString());
        }

        [Test]
        public void ShouldNot_PrintExcludedTypes_ByOptions()
        {
            var printer = ObjectPrinter<Person>.Configure(options => options.ForProperties<Guid>(x => x.Exclude()));

            result = printer.PrintToString(testObject);

            result.Should().NotContain(testObject.Id.ToString());
        }

        [Test]
        public void ShouldNot_PrintExcludedMembers_ByOptions()
        {
            var printer = ObjectPrinter<Person>.Configure(options => options.ForProperty(x => x.Name, x => x.Exclude()));

            result = printer.PrintToString(testObject);

            result.Should().NotContain(testObject.Name);
        }

        [Test]
        public void ShouldNot_PrintExcludedMembers()
        {
            var printer = ObjectPrinter<Person>.Configure(options => options.Exclude(x => x.Name));

            result = printer.PrintToString(testObject);

            result.Should().NotContain(testObject.Name);
        }

        [Test]
        public void SyntaxShowCase()
        {
            var obj = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter<Person>.Configure(
                                       options => options.Exclude<int>()
                                       .Exclude(person => person.Height)
                                       .ForProperties<double>(config => config.WithCulture(CultureInfo.InvariantCulture))
                                       .ForProperties<string>(config => config.WithTrimLength(10))
                                       .ForProperty(person => person.Name, config => config.WithSerializer(name => name.ToString()))
                                       .ForProperty(person => person.Id, config => config.WithCulture(CultureInfo.InvariantCulture))
                                       );

            result = printer.PrintToString(obj);
        }
    }
}