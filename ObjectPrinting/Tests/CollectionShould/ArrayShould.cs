using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using ObjectPrinting.Tests.TestClasses;

namespace ObjectPrinting.Tests.CollectionShould
{
    [TestFixture]
    public class ArrayShould
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
        public void PrintArrayCollection_WhenOneLineWithoutClassesWithFields()
        {
            var printer = ObjectPrinter.For<int[]>();
            var array = new int[] { 1, 2, 3, 5, 6, 7, 8, 0, 9, 6, 4, 3, 5, 6, 8 };
            printedString = printer.PrintToString(array);
            printedString.Should().Contain("[1, 2, 3, 5, 6, 7, 8, 0, 9, 6, 4, 3, 5, 6, 8]");
        }
        [Test]
        public void PrintArrayCollection_WhenHaveNesting()
        {
            var printer = ObjectPrinter.For<object[]>();
            var array = new object[] { 1, 2, 3, new List<int>() { 1, 2, 3 }, new List<string> { "a,e", "something" } };
            printedString = printer.PrintToString(array);
            printedString.Should().Contain("[1, 2, 3, [1, 2, 3], [a,e, something]]");
        }
        [Test]
        public void PrintArrayCollection_WhenHaveNestingDictionary()
        {
            var printer = ObjectPrinter.For<object[]>();
            var array = new object[] { new Dictionary<int, string>() { [1] = "edee", [2] = "rffe", [5] = "fffff" }, 1, 2, 3, new List<int>() { 1, 2, 3 }, new List<string> { "a,e", "something" } };
            printedString = printer.PrintToString(array);
            printedString.Should().Contain("[[[1] = edee, [2] = rffe, [5] = fffff], 1, 2, 3, [1, 2, 3], [a,e, something]]");
        }
        [Test]
        public void PrintArrayCollection_WhenHaveNestingArray()
        {
            var printer = ObjectPrinter.For<object[]>();
            var array = new object[] { new[] { 1, 2, 3, 4 }, new[] { 2, 3, 5, 5, 45, 4, 54, 54 }, new[] { 1336, 1337, 1338 } };
            printedString = printer.PrintToString(array);
            printedString.Should().Contain("[[1, 2, 3, 4], [2, 3, 5, 5, 45, 4, 54, 54], [1336, 1337, 1338]]");
        }
        [Test]
        public void PrintArrayCollection_WhenHaveNestingOtherIEnumerableCollection()
        {
            var printer = ObjectPrinter.For<object[]>();
            var array = new object[] { new HashSet<int>() { 1, 2, 3, 4 }, new HashSet<int>() { 2, 3, 5, 5, 45, 4, 54, 54 }, new HashSet<int>() { 1336, 1337, 1338 } };
            printedString = printer.PrintToString(array);
            printedString.Should().Contain("[[1, 2, 3, 4], [2, 3, 5, 45, 4, 54], [1336, 1337, 1338]]");//Cant repeat([[1, 2, 3, 4], [2, 3, 5, 5, 45, 4, 54, 54], [1336, 1337, 1338]]) because HashSet 
        }
        [Test]
        public void PrintArrayCollection_WhenHaveClassWithFields()
        {
            var printer = ObjectPrinter.For<Person[]>();
            var array = new Person[] { new Person() { Age = 22, Name = "Sirgey" } };
            printedString = printer.PrintToString(array);
            printedString.Should().Contain("[Person; Name = Sirgey; Age = 22; HaveCar = False]");
        }
        [Test]
        public void PrintArrayCollection_WhenHaveClassesWithFields()
        {
            var printer = ObjectPrinter.For<Person[]>();
            var array = new Person[] { new Person() { Age = 22, Name = "Sirgey" }, new Person() { Age = 24, Height = 155, Name = "Anna" } };
            printedString = printer.PrintToString(array);
            printedString.Should().Contain("[Person; Name = Sirgey; Age = 22; HaveCar = False, Person; Name = Anna; Height = 155; Age = 24; HaveCar = False]");
        }
        [Test]
        public void PrintArrayCollection_WhenHaveClassWithClassField()
        {
            var printer = ObjectPrinter.For<Person[]>();
            var array = new Person[] { new Person { Age = 22, Name = "Sirgey", Car = new Vehicle("BMW", 230, 1500, 2003) } };
            printedString = printer.PrintToString(array);
            printedString.Should().Contain("[Person; Name = Sirgey; Age = 22; HaveCar = True; Car = Vehicle; CarNumber = 0; BrandAuto = BMW; Power = 230; Weight = 1500; AgeOfCar = 19]");
        }

    }

}
