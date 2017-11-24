using System;
using System.Globalization;
using NUnit.Framework;
using FluentAssertions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinter_Should
    {
        private Person person;
        private Student student;

        [SetUp]
        public void SetUp()
        {
            person = new Person { Name = "Alexeeeeeeeey", Age = 19, Height = 178.5 };
            student = new Student
            {
                Name = "Alexeeeeeeeeeeeeeeeeeey",
                Age = 19,
                Height = 178.5,
                Number = 5,
                School = new School { Number = 5, Address = "Address" }
            };
        }

        [Test]
        public void UseCustomSeialization_ForType()
        {
            person.PrintToString(config => config.Printing<int>().Using(i => "this is int"))
                .Should().Contain("this is int");
        }

        [Test]
        public void UseCustomSerialization_ForProperty()
        {
            person.PrintToString(config => config.Printing(p => p.Age).Using(age => $"{age} y.o."))
                .Should().Contain("19 y.o.");
        }

        [Test]
        public void UseSpecifiedCulture_ForNumbers()
        {
            person.PrintToString(config => config.Printing<double>().WithCulture(new CultureInfo("RU-ru")))
                .Should().Contain("Height = 178,5");

            person.PrintToString(config => config.Printing<double>().WithCulture(CultureInfo.InvariantCulture))
                .Should().Contain("Height = 178.5");
        }

        [Test]
        public void ShrinkStrings_ToSpecifiedLength()
        {
            person.PrintToString(config => config.Printing<string>().ShrinkedToLength(4))
                .Should().Contain("Alex").And.NotContain("Alexeeeeeeeey");
        }

        [Test]
        public void OverrideTypeBehaviour_WithPropertyBehaviour()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(d => "type behaviour")
                .Printing(p => p.Age).Using(d => "property behaviour")
                .Build();

            printer.PrintToString(person)
                .Should().Contain("property behaviour").And.NotContain("type behaviour");
        }

        [Test]
        public void NotOverrideExclusion_WithDifferentBehaviour()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<int>()
                .Printing(p => p.Age).Using(age => "it shouldn't be printed")
                .Build();

            printer.PrintToString(person).Should().NotContain("it shouldn't be printed");
        }

        [Test]
        public void NotExcludeProperties_WithSameName()
        {
            var printer = ObjectPrinter.For<Student>()
                .Excluding(s => s.Number)
                .Printing(s => s.School.Number).Using(n => "it should be printed")
                .Build();

            printer.PrintToString(student).Should().Contain("it should be printed");
        }

        [Test]
        public void ApplyConfiguration_ToNestedProperties()
        {
            var printer = ObjectPrinter.For<Student>()
                .Excluding<Guid>()
                .Excluding<int>()
                .Printing<string>().ShrinkedToLength(4)
                .Build();

            printer.PrintToString(student)
                .Should().NotContain("Number").And.Contain("Address = Addr" + Environment.NewLine);

        }
    }
}