using System;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private string actual;
        private Person person;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            person = new Person
            {
                Name = "Alex",
                Age = 19,
                Birthday = DateTime.MinValue,
                Height = 178.6,
                Id = Guid.NewGuid(),
                Index = 'A'
            };
        }
        
        [Test]
        public void AlternativeSerialisationPropertyTest()
        {
            var printer = Config<Person>.CreateConfig()
                .SetAlternativeSerialisation().For(x => x.Birthday).WithMethod(dt => $"{dt.Day}.{dt.Month} - {dt.Year}")
                .Configure();
            actual = printer.PrintToString(person);

            actual.Should().Contain("Birthday = 1.1 - 1");
        }
        
        [Test]
        public void AlternativeSerialisationTypeTest()
        {
            var printer = Config<Person>.CreateConfig()
                .SetAlternativeSerialisation().For<string>().WithMethod(x => $"([{x}])")
                .Configure();
            actual = printer.PrintToString(person);

            actual.Should().Contain("Name = ([Alex]d)");
        }
        
        [Test]
        public void CultureTypeTest()
        {
            var printer = Config<Person>.CreateConfig()
                .SetCulture().For<double>(CultureInfo.CurrentUICulture)
                .Configure();
            actual = printer.PrintToString(person);

            actual.Should().Contain("Height = 178,6");
        }
        
        [Test]
        public void CultureAllOthersTest()
        {
            var printer = Config<Person>.CreateConfig()
                .SetCulture().ForAllOthers(CultureInfo.CurrentUICulture)
                .Configure();
            actual = printer.PrintToString(person);

            actual.Should().Contain("Height = 178,6");
        }
        
        [Test]
        public void IgnoreCombineTest()
        {
            var printer = Config<Person>.CreateConfig()
                .Ignore()
                    .Property(x => x.Birthday).And
                    .Type<double>().And
                    .Property(x => x.Id)
                .Configure();
            actual = printer.PrintToString(person);

            actual.Split(Environment.NewLine).Where(x => x != "").Should().HaveCount(5);
            actual.Should().NotContain("Birthday = ");
            actual.Should().NotContain("Height = ");
            actual.Should().NotContain("Id = ");
        }
        
        [Test]
        public void IgnorePropertyTest()
        {
            var printer = Config<Person>.CreateConfig()
                .Ignore().Property(x => x.Birthday)
                .Configure();
            actual = printer.PrintToString(person);

            actual.Split(Environment.NewLine).Where(x => x != "").Should().HaveCount(7);
            actual.Should().NotContain("Birthday = ");
        }

        [Test]
        public void IgnoreTypeTest()
        {
            var printer = Config<Person>.CreateConfig()
                .Ignore().Type<Guid>()
                .Configure();
            actual = printer.PrintToString(person);

            actual.Split(Environment.NewLine).Where(x => x != "").Should().HaveCount(7);
            actual.Should().NotContain("Id = ");
        }
        
        [Test]
        public void Demo()
        {
            var personDemo = new Person
            {
                Name = "  Alex ", 
                Age = 19, 
                Birthday = DateTime.Today,
                Height = 178.6, 
                Id = Guid.NewGuid(),
                Index = 'A',
                Parent = new Person
                {
                    Name = "Bob", 
                    Age = 52, 
                    Birthday = DateTime.Now,
                    Height = 189.2, 
                    Id = Guid.NewGuid(),
                    Index = 'B',
                    Parent = new Person
                    {
                        Name = "Juan Pablo Montoya", 
                        Age = 78, 
                        Birthday = DateTime.MinValue,
                        Height = 158.7, 
                        Index = 'C',
                        Id = Guid.NewGuid()
                    }
                }
            };
            personDemo.Parent.Parent.Parent = personDemo;
            
            var printerDemo = Config<Person>.CreateConfig()
                .SetCulture()
                    .For<double>(CultureInfo.CurrentUICulture).And
                    .ForAllOthers(CultureInfo.InvariantCulture)
                .Ignore()
                    .Property(p => p.Age).And
                    .Type<char>().InAllNestingLevels()
                .SetAlternativeSerialisation()
                    .For<DateTime>().WithMethod(dt => $"{dt.Day}.{dt.Month} - {dt.Year}").And
                    .For(p => p.Name).WithMethod(n => n.Trim().ToUpper()).WithCharsLimit(10).And
                    .For<Guid>().WithMethod(x => x.ToString("N"))
                .Configure();

            Console.WriteLine(printerDemo.PrintToString(personDemo));
        }
    }
}