using NUnit.Framework;
using NUnit.Framework.Interfaces;
using ObjectPrinting.Tests.TestClasses;
using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;

namespace ObjectPrinting.Tests.CollectionShould
{
    [TestFixture]
    public class ListShould
    {
        private string printedString;
        public Vehicle VehicleCar;
        private Person simplePerson;
        private Person childPerson;

        [SetUp]
        public void SetUp()
        {
            VehicleCar = new Vehicle("Audi", 230, 1400, 2005);

            simplePerson = new Person() { Age = 35, Height = 155, Id = new Guid(), Name = "Anna", Car = VehicleCar };
            childPerson = new Person()
            {
                Age = 8,
                Height = 86.34,
                Id = new Guid(),
                Name = "Seraphina",
                Parent = simplePerson
            };
            printedString = null;

        }

        [TearDown]
        public void TearDown()
        {
            if (TestContext.CurrentContext.Result.Outcome.Status != TestStatus.Failed)
                Console.WriteLine(printedString);
        }
        [Test]
        public void PrintListCollection_WhenOneLineWithoutClassesWithFields()
        {
            var printer = ObjectPrinter.For<List<int>>();
            var list = new List<int>() { 1, 2, 3, 5, 6, 7, 8, 0, 9, 6, 4, 3, 5, 6, 8 };
            printedString = printer.PrintToString(list);
            printedString.Should().Contain("[1, 2, 3, 5, 6, 7, 8, 0, 9, 6, 4, 3, 5, 6, 8]");
        }
        [Test]
        public void PrintListCollection_WhenHaveNesting()
        {
            var printer = ObjectPrinter.For<List<object>>();
            var list = new List<object> { 1, 2, 3, new List<int>() { 1, 2, 3 }, new List<string> { "a,e", "something" } };
            printedString = printer.PrintToString(list);
            printedString.Should().Contain("[1, 2, 3, [1, 2, 3], [a,e, something]]");
        }
        [Test]
        public void PrintListCollection_WhenHaveNestingDictionary()
        {
            var printer = ObjectPrinter.For<List<object>>();
            var list = new List<object> { new Dictionary<int, string>() { [1] = "edee", [2] = "rffe", [5] = "fffff" }, 1, 2, 3, new List<int>() { 1, 2, 3 }, new List<string> { "a,e", "something" } };
            printedString = printer.PrintToString(list);
            printedString.Should().Contain("[[[1] = edee, [2] = rffe, [5] = fffff], 1, 2, 3, [1, 2, 3], [a,e, something]]");
        }
        [Test]
        public void PrintListCollection_WhenHaveNestingArray()
        {
            var printer = ObjectPrinter.For<List<int[]>>();
            var list = new List<int[]> { new[] { 1, 2, 3, 4 }, new[] { 2, 3, 5, 5, 45, 4, 54, 54 }, new[] { 1336, 1337, 1338 } };
            printedString = printer.PrintToString(list);
            printedString.Should().Contain("[[1, 2, 3, 4], [2, 3, 5, 5, 45, 4, 54, 54], [1336, 1337, 1338]]");
        }
        [Test]
        public void PrintListCollection_WhenHaveNestingOtherIEnumerableCollection()
        {
            var printer = ObjectPrinter.For<List<HashSet<int>>>();
            var list = new List<HashSet<int>> { new HashSet<int>() { 1, 2, 3, 4 }, new HashSet<int>() { 2, 3, 5, 5, 45, 4, 54, 54 }, new HashSet<int>() { 1336, 1337, 1338 } };
            printedString = printer.PrintToString(list);
            printedString.Should().Contain("[[1, 2, 3, 4], [2, 3, 5, 45, 4, 54], [1336, 1337, 1338]]");//Cant repeat([[1, 2, 3, 4], [2, 3, 5, 5, 45, 4, 54, 54], [1336, 1337, 1338]]) because HashSet 
        }
        [Test]
        public void PrintListCollection_WhenHaveClassWithFields()
        {
            var printer = ObjectPrinter.For<List<Person>>();
            var list = new List<Person> { new Person() { Age = 22, Name = "Sirgey" } };
            printedString = printer.PrintToString(list);
            printedString.Should().Contain("[Person; Name = Sirgey; Age = 22; HaveCar = False]");
        }
        [Test]
        public void PrintListCollection_WhenHaveClassesWithFields()
        {
            var printer = ObjectPrinter.For<List<Person>>();
            var list = new List<Person> { new Person() { Age = 22, Name = "Sirgey" }, new Person() { Age = 24, Height = 155, Name = "Anna" } };
            printedString = printer.PrintToString(list);
            printedString.Should().Contain("[Person; Name = Sirgey; Age = 22; HaveCar = False, Person; Name = Anna; Height = 155; Age = 24; HaveCar = False]");
        }
        [Test]
        public void PrintListCollection_WhenHaveClassWithClassField()
        {
            var printer = ObjectPrinter.For<List<Person>>();
            var list = new List<Person> { new Person { Age = 22, Name = "Sirgey", Car = new Vehicle("BMW", 230, 1500, 2003) } };
            printedString = printer.PrintToString(list);
            printedString.Should().Contain("[Person; Name = Sirgey; Age = 22; HaveCar = True; Car = Vehicle; CarNumber = 0; BrandAuto = BMW; Power = 230; Weight = 1500; AgeOfCar = 19]");
        }
    }
}
