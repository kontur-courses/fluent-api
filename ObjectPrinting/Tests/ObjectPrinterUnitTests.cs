using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterUnitTests
    {
        private Person person = new Person(Guid.NewGuid(), "Alex", 182, 24);
        private Point point = new Point(1.2, -5.1, 71.12f);

        private Animal animal = new Animal("Tyzik", "Dog",
            new Animal("Bob", "Cat",
                new Animal("Joe", "Monkey")));

        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void ExcludingTypes()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<string>();

            printer.PrintToString(person).Should().Be($"Person{Environment.NewLine}" +
                                                      $"\tId = {person.Id}{Environment.NewLine}" +
                                                      $"\tHeight = {person.Height}{Environment.NewLine}" +
                                                      $"\tAge = {person.Age}{Environment.NewLine}");
        }

        [Test]
        public void ExcludingProperties()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(person => person.Height);

            printer.PrintToString(person).Should().Be($"Person{Environment.NewLine}" +
                                                      $"\tId = {person.Id}{Environment.NewLine}" +
                                                      $"\tName = {person.Name}{Environment.NewLine}" +
                                                      $"\tAge = {person.Age}{Environment.NewLine}");
        }

        [Test]
        public void AlternativeSerializationForSpecificType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(x => (x + 1).ToString());

            printer.PrintToString(person).Should().Be($"Person{Environment.NewLine}" +
                                                      $"\tId = {person.Id}{Environment.NewLine}" +
                                                      $"\tName = {person.Name}{Environment.NewLine}" +
                                                      $"\tHeight = {person.Height}{Environment.NewLine}" +
                                                      $"\tAge = {person.Age + 1}{Environment.NewLine}");
        }

        [Test]
        public void CultureSpecificationForTypes()
        {
            var printer = ObjectPrinter.For<Point>()
                .Printing<float>().UsingCulture(CultureInfo.GetCultureInfo("ru-Ru"));

            printer.PrintToString(point).Should().Be(
                $"Point{Environment.NewLine}" +
                $"\tX = {point.X}{Environment.NewLine}" +
                $"\tY = {point.Y}{Environment.NewLine}" +
                $"\tZ = {point.Z.ToString(CultureInfo.GetCultureInfo("ru-Ru"))}{Environment.NewLine}");
        }

        [Test]
        public void AlternativeSerializationForSpecificProperty()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Age).Using(age => (age * 2).ToString());

            printer.PrintToString(person).Should().Be($"Person{Environment.NewLine}" +
                                                      $"\tId = {person.Id}{Environment.NewLine}" +
                                                      $"\tName = {person.Name}{Environment.NewLine}" +
                                                      $"\tHeight = {person.Height}{Environment.NewLine}" +
                                                      $"\tAge = {person.Age * 2}{Environment.NewLine}");
        }

        [Test]
        public void StringTrimmingForSpecificProperty()
        {
            var printer = ObjectPrinter.For<Animal>()
                .Printing(animal => animal.Name).TrimmedToLength(2);

            printer.PrintToString(animal).Should().Be($"Animal{Environment.NewLine}" +
                                                      $"\tName = {animal.Name.Substring(0, 2)}{Environment.NewLine}" +
                                                      $"\tKind = {animal.Kind}{Environment.NewLine}" +
                                                      $"\tParent = Animal{Environment.NewLine}" +
                                                      $"\t\tName = {animal.Parent.Name.Substring(0, 2)}{Environment.NewLine}" +
                                                      $"\t\tKind = {animal.Parent.Kind}{Environment.NewLine}" +
                                                      $"\t\tParent = Animal{Environment.NewLine}" +
                                                      $"\t\t\tName = {animal.Parent.Parent.Name.Substring(0, 2)}{Environment.NewLine}" +
                                                      $"\t\t\tKind = {animal.Parent.Parent.Kind}{Environment.NewLine}" +
                                                      $"\t\t\tParent = null{Environment.NewLine}");
        }

        [Test]
        public void AllStringsTrimming()
        {
            var printer = ObjectPrinter.For<Animal>()
                .TrimmedToLength(2);

            printer.PrintToString(animal).Should().Be($"Animal{Environment.NewLine}" +
                                                      $"\tName = {animal.Name.Substring(0, 2)}{Environment.NewLine}" +
                                                      $"\tKind = {animal.Kind.Substring(0, 2)}{Environment.NewLine}" +
                                                      $"\tParent = Animal{Environment.NewLine}" +
                                                      $"\t\tName = {animal.Parent.Name.Substring(0, 2)}{Environment.NewLine}" +
                                                      $"\t\tKind = {animal.Parent.Kind.Substring(0, 2)}{Environment.NewLine}" +
                                                      $"\t\tParent = Animal{Environment.NewLine}" +
                                                      $"\t\t\tName = {animal.Parent.Parent.Name.Substring(0, 2)}{Environment.NewLine}" +
                                                      $"\t\t\tKind = {animal.Parent.Parent.Kind.Substring(0, 2)}{Environment.NewLine}" +
                                                      $"\t\t\tParent = null{Environment.NewLine}");
        }

        [Test]
        public void CyclicReferences_ShouldNotCauseStackOverflow()
        {
            var cyclicAnimal1 = new Animal("Peter", "Dog");
            var cyclicAnimal2 = new Animal("Jack", "Cat");
            cyclicAnimal1.Parent = cyclicAnimal2;
            cyclicAnimal2.Parent = cyclicAnimal1;
            var printer = ObjectPrinter.For<Animal>();

            Assert.DoesNotThrow(() => printer.PrintToString(cyclicAnimal1));
        }

        [Test]
        public void ListsSerialization()
        {
            var student = new Student("Vasya");
            student.Friends = new List<string>();
            student.Friends.Add("Ivan");
            student.Friends.Add("Anton");
            student.Friends.Add("Konstantin");

            var printer = ObjectPrinter.For<Student>();

            printer.PrintToString(student).Should().Be($"Student{Environment.NewLine}" +
                                                       $"\tName = {student.Name}{Environment.NewLine}" +
                                                       $"\tFriends = {Environment.NewLine}" +
                                                       $"\t\t[{Environment.NewLine}" +
                                                       $"\t\t{student.Friends[0]}{Environment.NewLine}" +
                                                       $"\t\t{student.Friends[1]}{Environment.NewLine}" +
                                                       $"\t\t{student.Friends[2]}{Environment.NewLine}" +
                                                       $"\t\t]{Environment.NewLine}" +
                                                       $"\tFavoriteRealValues = null{Environment.NewLine}" +
                                                       $"\tMarks = null{Environment.NewLine}");
        }

        [Test]
        public void DictionarySerialization()
        {
            var student = new Student("Vasya");
            student.Marks = new Dictionary<string, int>();
            student.Marks["math"] = 5;
            student.Marks["biology"] = 3;

            var printer = ObjectPrinter.For<Student>();
            printer.PrintToString(student).Should().Be($"Student{Environment.NewLine}" +
                                                       $"\tName = {student.Name}{Environment.NewLine}" +
                                                       $"\tFriends = null{Environment.NewLine}" +
                                                       $"\tFavoriteRealValues = null{Environment.NewLine}" +
                                                       $"\tMarks = {Environment.NewLine}" +
                                                       $"\t\t[{Environment.NewLine}" +
                                                       $"\t\tmath : 5{Environment.NewLine}" +
                                                       $"\t\tbiology : 3{Environment.NewLine}" +
                                                       $"\t\t]{Environment.NewLine}");
        }

        [Test]
        public void ArraySerialization()
        {
            var student = new Student("Vasya");
            student.FavoriteRealValues = new[] {1.2, 43, -54.123};

            var printer = ObjectPrinter.For<Student>();
            printer.PrintToString(student).Should().Be($"Student{Environment.NewLine}" +
                                                       $"\tName = {student.Name}{Environment.NewLine}" +
                                                       $"\tFriends = null{Environment.NewLine}" +
                                                       $"\tFavoriteRealValues = {Environment.NewLine}" +
                                                       $"\t\t[{Environment.NewLine}" +
                                                       $"\t\t{student.FavoriteRealValues[0]}{Environment.NewLine}" +
                                                       $"\t\t{student.FavoriteRealValues[1]}{Environment.NewLine}" +
                                                       $"\t\t{student.FavoriteRealValues[2]}{Environment.NewLine}" +
                                                       $"\t\t]{Environment.NewLine}" +
                                                       $"\tMarks = null{Environment.NewLine}");
        }

        [Test]
        public void CollectionSerialization_ShouldApplyAllSpecifications()
        {
            var student = new Student("Vasya");
            student.FavoriteRealValues = new[] {1.2, 43, 2.1};
            student.Marks = new Dictionary<string, int>();
            student.Marks["math"] = 5;
            student.Marks["biology"] = 3;

            var printer = ObjectPrinter.For<Student>()
                .Printing<double>().UsingCulture(CultureInfo.GetCultureInfo("ru-Ru"));

            printer.PrintToString(student).Should().Be($"Student{Environment.NewLine}" +
                                                       $"\tName = {student.Name}{Environment.NewLine}" +
                                                       $"\tFriends = null{Environment.NewLine}" +
                                                       $"\tFavoriteRealValues = {Environment.NewLine}" +
                                                       $"\t\t[{Environment.NewLine}" +
                                                       $"\t\t1,2{Environment.NewLine}" +
                                                       $"\t\t43{Environment.NewLine}" +
                                                       $"\t\t2,1{Environment.NewLine}" +
                                                       $"\t\t]{Environment.NewLine}" +
                                                       $"\tMarks = {Environment.NewLine}" +
                                                       $"\t\t[{Environment.NewLine}" +
                                                       $"\t\tmath : 5{Environment.NewLine}" +
                                                       $"\t\tbiology : 3{Environment.NewLine}" +
                                                       $"\t\t]{Environment.NewLine}");
        }
    }
}