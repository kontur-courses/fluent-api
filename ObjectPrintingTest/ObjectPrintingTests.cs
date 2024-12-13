using System.Globalization;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrintingTest
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovalFiles")]
    public class ObjectPrinting_Tests
    {
        [Test]
        public void PrintToString_ShouldMatchApproved_DefaultConfiguration()
        {
            var person = new Person { Name = "Alex", Age = 19, BirthDate = new DateTime(2021, 1, 21) };

            var printer = ObjectPrinter.For<Person>();
            var result = printer.PrintToString(person);

            Approvals.Verify(result);
        }

        [Test]
        public void ObjectPrinter_ShouldExcludeFieldOrProperty_WithPassedType()
        {
            var person = new Person { Name = "Alex", Age = 19, BirthDate = new DateTime(2021, 1, 21) };

            var printer = ObjectPrinter
                .For<Person>()
                .ForType<string>()
                .Exclude();


            Approvals.Verify(printer.PrintToString(person));
        }

        [Test]
        public void ObjectPrinter_ShouldAlternativeSerializer_ForPassedType()
        {
            var person = new Person
                { Name = "Alex", Surname = "White", Age = 19, BirthDate = new DateTime(2021, 1, 21) };

            var printer = ObjectPrinter
                .For<Person>()
                .ForType<string>().Using(s => $"String field: {s}")
                .ForType<int>().Using(n => $"Int field: {n}");


            Approvals.Verify(printer.PrintToString(person));
        }

        [Test]
        public void ObjectPrinter_ShouldAddCulture_ForDouble()
        {
            var person = new Person { Name = "Alex", Surname = "White", Age = 19, Height = 199.9 };

            var printer = ObjectPrinter.For<Person>()
                .ForProperty(p => p.Height)
                .WithCulture(new CultureInfo("en-US"));


            Approvals.Verify(printer.PrintToString(person));
        }

        [Test]
        public void ObjectPrinter_ShouldAddCulture_ForDateTime()
        {
            var person = new Person
                { Name = "Alex", Surname = "White", Age = 19, BirthDate = new DateTime(2021, 1, 21) };

            var printer = ObjectPrinter.For<Person>()
                .ForProperty(p => p.BirthDate)
                .WithCulture(CultureInfo.GetCultureInfo("en-US"));

            Approvals.Verify(printer.PrintToString(person));
        }

        [Test]
        public void ObjectPrinter_ShouldAlternativeSerializer_ForSpecificPropertyOrField()
        {
            var person = new Person
                { Name = "Alex", Surname = "White", Age = 19, BirthDate = new DateTime(2021, 1, 21) };

            var printer = ObjectPrinter.For<Person>()
                .ForProperty(p => p.Name).Using(name => $"My name is {name}")
                .ForProperty(p => p.BirthDate).Using(date => date.ToString("M.dd"));

            Approvals.Verify(printer.PrintToString(person));
        }

        [Test]
        public void ObjectPrinter_ShouldTruncateSerializer_ForString()
        {
            var person = new Person
                { Name = "Alex", Surname = "White", Age = 19, BirthDate = new DateTime(2021, 1, 21) };

            var printer = ObjectPrinter.For<Person>()
                .ForProperty(obj => obj.Name).TrimTo(2);

            Approvals.Verify(printer.PrintToString(person));
        }

        [Test]
        public void ObjectPrinter_ShouldExclude_SpecificProperty()
        {
            var person = new Person
                { Name = "Alex", Surname = "White", Age = 19, BirthDate = new DateTime(2021, 1, 21) };

            var printer = ObjectPrinter.For<Person>()
                .ForProperty(p => p.Name).Exclude();


            Approvals.Verify(printer.PrintToString(person));
        }

        [Test]
        public void ObjectPrinter_ShouldPrintField_ForInnerClass()
        {
            var person = new Person
            {
                Name = "Alex",
                Surname = "White",
                Age = 19,
                BirthDate = new DateTime(2021, 1, 21),
                Parent = new Person { Name = "Sam" }
            };

            var printer = ObjectPrinter.For<Person>();

            Approvals.Verify(printer.PrintToString(person));
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectPrintArray()
        {
            var list = new[] { new Person { Name = "Mary" }, new Person { Name = "Alice" } };

            var printer = ObjectPrinter.For<Person[]>();

            Approvals.Verify(printer.PrintToString(list));
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectPrintList()
        {
            var list = new List<Person> { new Person { Name = "Mary" }, new Person { Name = "Alice" } };

            var printer = ObjectPrinter.For<List<Person>>();

            Approvals.Verify(printer.PrintToString(list));
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectPrintDictionary()
        {
            var list = new Dictionary<string, Person>
            {
                { "person1", new Person { Name = "Mary" } },
                { "person2", new Person { Name = "Alice" } }
            };

            var printer = ObjectPrinter.For<Dictionary<string, Person>>();

            Approvals.Verify(printer.PrintToString(list));
        }

        [Test] //7
        public void ObjectPrinter_ShouldCorrectHandleCycleReference()
        {
            var a = new Person() { };
            a.Parent = a;
            
            var printer = ObjectPrinter.For<Person>();
            Func<string> act = () => printer.PrintToString(a);

            act.Should().NotThrow<StackOverflowException>();
            Approvals.Verify(printer.PrintToString(a));
        }
    }
}