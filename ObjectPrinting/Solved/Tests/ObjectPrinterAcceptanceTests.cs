using FluentAssertions;
using System;
using System.Globalization;
using NUnit.Framework;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace ObjectPrinting.Solved.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
       
        [Test]
        public void ExcludePropertiesByType()
        {
            var person = new Person() { Name = "Vova", Age = 21, Height = 190, Id = new Guid(), Health =100 };
            var printer = ObjectPrinter
                .For<Person>()
                .Excluding<int>();

            var res = printer.PrintToString(person, 1);

            res.Should().Be("Person" +
                Environment.NewLine +
                "\tId = Guid" +
                Environment.NewLine+
                "\tName = Vova" +
                Environment.NewLine +
                "\tHeight = 190" +
                Environment.NewLine +
                "\tDateOfBirth = 01.01.0001 0:00:00" +
                 Environment.NewLine+
                  "\tParent = null" +
                Environment.NewLine);
        }

        [Test]
        public void PruneStringProperty()
        {
            var person = new Person() { Name = "Volodya", Age = 21, Height = 190, Id = new Guid(), Health = 100 };
            var printer = ObjectPrinter
                .For<Person>()
                .Printing(person => person.Name).TrimmedToLength(3);

            var res = printer.PrintToString(person, 1);

            res.Should().Be("Person" +
                 Environment.NewLine +
                 "\tId = Guid" +
                 Environment.NewLine +
                 "\tName = Vol" +
                 Environment.NewLine +
                 "\tHeight = 190" +
                 Environment.NewLine +
                 "\tAge = 21" +
                 Environment.NewLine +
                 "\tHealth = 100" +
                 Environment.NewLine +
                 "\tDateOfBirth = 01.01.0001 0:00:00" +
                 Environment.NewLine +
                  "\tParent = null" +
                 Environment.NewLine);
        }

        [Test]
        public void AlternativePrintForProp()
        {
            var person = new Person() { Name = "Vova", Age = 21, Height = 190, Id = new Guid(), Health = 100 };
            var printer = ObjectPrinter
                .For<Person>()
                .Printing(person=>person.Health).Using(health=>$"{health}/100");

            var res = printer.PrintToString(person, 1);

            res.Should().Be("Person" +
                Environment.NewLine +
                "\tId = Guid" +
                Environment.NewLine +
                "\tName = Vova" +
                Environment.NewLine +
                "\tHeight = 190" +
                Environment.NewLine +
                 "\tAge = 21" +
                Environment.NewLine +
                "\tHealth = 100/100" +
                Environment.NewLine +
                "\tDateOfBirth = 01.01.0001 0:00:00" +
                 Environment.NewLine +
                  "\tParent = null" +
                Environment.NewLine);
        }

        [Test]
        public void AlternativePrintForType()
        {
            var person = new Person() { Name = "Vova", Age = 21, Height = 190, Id = new Guid(), Health = 100 };
            var printer = ObjectPrinter
                .For<Person>()
                .Printing<int>().Using(i => $"(целое число){i}");

            var res = printer.PrintToString(person, 1);

            res.Should().Be("Person" +
                Environment.NewLine +
                "\tId = Guid" +
                Environment.NewLine +
                "\tName = Vova" +
                Environment.NewLine +
                "\tHeight = 190" +
                Environment.NewLine +
                "\tAge = (целое число)21" +
                Environment.NewLine +
                "\tHealth = (целое число)100" +
                Environment.NewLine +
                "\tDateOfBirth = 01.01.0001 0:00:00" +
                Environment.NewLine+
                 "\tParent = null" +
                Environment.NewLine);
        }

        [Test]
        public void ChangeCultureForDouble()
        {
            var person = new Person() { Name = "Vova", Age = 21, Height = 190.5, Id = new Guid(), Health = 100, DateOfBirth = new DateTime(2000, 1, 18) };
            var printer = ObjectPrinter
                .For<Person>()
                .Printing<double>().UsingCulture(CultureInfo.InvariantCulture);

            var res = printer.PrintToString(person, 1);

            res.Should().Be("Person" +
                Environment.NewLine +
                "\tId = Guid" +
                Environment.NewLine +
                "\tName = Vova" +
                Environment.NewLine +
                "\tHeight = 190.5" +
                Environment.NewLine +
                "\tAge = 21" +
                Environment.NewLine +
                "\tHealth = 100" +
                Environment.NewLine +
                "\tDateOfBirth = 18.01.2000 0:00:00" +
                Environment.NewLine +
                "\tParent = null" +
                Environment.NewLine);
        }

        [Test]
        public void ChangeCultureForDateTime()
        {
            var person = new Person() { Name = "Vova", Age = 21, Height = 190.5, Id = new Guid(), Health = 100, DateOfBirth = new DateTime(2000, 1, 18) };
            var printer = ObjectPrinter
                .For<Person>()
                .Printing<DateTime>().UsingCulture(CultureInfo.InvariantCulture);

            var res = printer.PrintToString(person, 1);

            res.Should().Be("Person" +
                Environment.NewLine +
                "\tId = Guid" +
                Environment.NewLine +
                "\tName = Vova" +
                Environment.NewLine +
                "\tHeight = 190,5" +
                Environment.NewLine +
                "\tAge = 21" +
                Environment.NewLine +
                "\tHealth = 100" +
                Environment.NewLine +
                "\tDateOfBirth = 01/18/2000 00:00:00" +
                Environment.NewLine +
                 "\tParent = null" +
                Environment.NewLine);
        }



        [Test]
        public void ExcludeProp()
        {
            var person = new Person() { Name = "Vova", Age = 21, Height = 190.5, Id = new Guid(), Health = 100 };
            var printer = ObjectPrinter.For<Person>()
                .Excluding(person=>person.Age);

            var res = printer.PrintToString(person,1);

            res.Should().Be("Person" +
               Environment.NewLine +
               "\tId = Guid" +
               Environment.NewLine +
               "\tName = Vova" +
               Environment.NewLine +
               "\tHeight = 190,5" +
               Environment.NewLine +
               "\tHealth = 100" +
               Environment.NewLine +
               "\tDateOfBirth = 01.01.0001 0:00:00" +
               Environment.NewLine +
               "\tParent = null" +
               Environment.NewLine);
        }

        [Test]
        public void ExcludeInnerProp()
        {
            var person = new Person() 
            { 
                Name = "Vova",
                Age = 21,
                Height = 190.5,
                Id = new Guid(),
                Health = 100,
                Parent = new Person() {Name ="Papa", Age = 45, Health = 1000 } 
            };
            var expectedRes = "Person" +
                Environment.NewLine +
                "\tId = Guid" +
                Environment.NewLine +
                "\tName = Vova" +
                Environment.NewLine +
                "\tHeight = 190,5" +
                Environment.NewLine +
                "\tAge = 21" +
                Environment.NewLine +
                "\tHealth = 100" +
                Environment.NewLine +
                "\tDateOfBirth = 01.01.0001 0:00:00" +
                Environment.NewLine +
                "\tParent = Person" +
                Environment.NewLine +
                "\t\tId = Guid" +
                Environment.NewLine +
                "\t\tName = Papa" +
                Environment.NewLine +
                "\t\tHeight = 0" +
                Environment.NewLine +
                "\t\tHealth = 1000" +
                Environment.NewLine +
                "\t\tDateOfBirth = 01.01.0001 0:00:00" +
                Environment.NewLine +
                "\t\tParent = null" +
                Environment.NewLine;
            var printer = ObjectPrinter.For<Person>()
                .Excluding(person => person.Parent.Age);

            var res = printer.PrintToString(person, 1);

            res.Should().Be(expectedRes);
        }

        [Test]
        public void ShouldNotThrowExceptionWhenCircularLinks()
        {
            var person = new Person()
            {
                Name = "Vova",
                Age = 21,
                Height = 190.5,
                Id = new Guid(),
                Health = 100,
            };
            var parent = new Person()
            {
                Name = "Father",
                Parent = person
            };
            person.Parent = parent;
            var printer = ObjectPrinter.For<Person>()
                .Excluding(person => person.Parent.Age);

            Action act = () => printer.PrintToString(person, 10);

            act.Should().NotThrow();
        }



        private string PrintToString(object obj, int nestingLevel, int maxNestingLevel)
        {

            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();

            sb.AppendLine(type.Name);

            if (nestingLevel > maxNestingLevel)
                return sb.ToString();

            foreach (var propertyInfo in type.GetProperties())
            {

                sb.Append(identation + propertyInfo.Name + " = " +
                        PrintToString(propertyInfo.GetValue(obj),
                            nestingLevel + 1, maxNestingLevel));

            }

            return sb.ToString();
        }
    }

}