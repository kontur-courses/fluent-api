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
    public class DictionaryShould
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
        public void PrintDictionaryCollection_WhenOneLineWithoutClassesWithFields()
        {
            var printer = ObjectPrinter.For<Dictionary<int,int>>();
            var dictionary = new Dictionary<int, int>() { [1] = 2, [3] = 4, [5] = 6 };
            printedString = printer.PrintToString(dictionary);
            printedString.Should().Contain("[[1] = 2, [3] = 4, [5] = 6]");
        }
        [Test]
        public void PrintDictionaryCollection_WhenHaveNestingAndListKeyForDictionary()
        {
            var printer = ObjectPrinter.For<Dictionary<object,object>>();
            var dictionary = new Dictionary<object, object>() { [1]= 2, [3]=new List<int>() { 1, 2, 3 }, [new List<string> { "a,e", "something" }]=2};
            printedString = printer.PrintToString(dictionary);
            printedString.Should().Contain("[[1] = 2, [3] = [1, 2, 3], [[a,e, something]] = 2]");
        }
        [Test]
        public void PrintDictionaryCollection_WhenHaveNestingDictionary()
        {
            var printer = ObjectPrinter.For<Dictionary<object,object>>();
            var dictionary = new Dictionary<object,object> { [1] =new Dictionary<object, object>() { [1] = "edee", [2] = "rffe", [5] = "fffff" },[ new Dictionary<int, string>() { [1] = "edee", [2] = "rffe", [5] = "fffff" }]= 1, [2]=3,[3]= new List<int>() { 1, 2, 3 },["3f"]= new List<string> { "a,e", "something" } };
            printedString = printer.PrintToString(dictionary);
            printedString.Should().Contain("[[1] = [[1] = edee, [2] = rffe, [5] = fffff], [[[1] = edee, [2] = rffe, [5] = fffff]] = 1, [2] = 3, [3] = [1, 2, 3], [3f] = [a,e, something]]");
        }
        [Test]
        public void PrintDictionaryCollection_WhenHaveNestingArray()
        {
            var printer = ObjectPrinter.For<Dictionary<object, int[]>>();
            var dictionary = new Dictionary<object, int[]>{ [2]=new[] { 1, 2, 3, 4 },[1]= new[] { 2, 3, 5, 5, 45, 4, 54, 54 },[55]= new[] { 1336, 1337, 1338 } };
            printedString = printer.PrintToString(dictionary);
            printedString.Should().Contain("[[2] = [1, 2, 3, 4], [1] = [2, 3, 5, 5, 45, 4, 54, 54], [55] = [1336, 1337, 1338]]");
        }
        [Test]
        public void PrintDictionaryCollection_WhenHaveNestingOtherIEnumerableCollection()
        {
            var printer = ObjectPrinter.For<Dictionary<object, HashSet<int>>>();
            var dictionary = new Dictionary<object, HashSet<int>> {[1]= new HashSet<int>() { 1, 2, 3, 4 },[2] =new HashSet<int>() { 2, 3, 5, 5, 45, 4, 54, 54 }, [3]=new HashSet<int>() { 1336, 1337, 1338 } };
            printedString = printer.PrintToString(dictionary);
            printedString.Should().Contain("[[1] = [1, 2, 3, 4], [2] = [2, 3, 5, 45, 4, 54], [3] = [1336, 1337, 1338]]");//Cant repeat([[1, 2, 3, 4], [2, 3, 5, 5, 45, 4, 54, 54], [1336, 1337, 1338]]) because HashSet 
        }
        [Test]
        public void PrintDictionaryCollection_WhenHaveClassWithFields()
        {
            var printer = ObjectPrinter.For<Dictionary<object, Person>>();
            var dictionary = new Dictionary<object, Person> { [2]= new Person() { Age = 22, Name = "Sirgey" } };
            printedString = printer.PrintToString(dictionary);
            printedString.Should().Contain("[[2] = Person; Name = Sirgey; Age = 22; HaveCar = False]");
        }
        [Test]
        public void PrintDictionaryCollection_WhenHaveClassesWithFields()
        {
            var printer = ObjectPrinter.For<Dictionary<object, Person>>();
            var dictionary = new Dictionary<object, Person> {["PetrPerson"] = new Person() { Age = 22, Name = "Sirgey" }, ["PetrPerson2"] = new Person() { Age = 24, Height = 155, Name = "Anna" } };
            printedString = printer.PrintToString(dictionary);
            printedString.Should().Contain("[[PetrPerson] = Person; Name = Sirgey; Age = 22; HaveCar = False, [PetrPerson2] = Person; Name = Anna; Height = 155; Age = 24; HaveCar = False]");
        }
        [Test]
        public void PrintDictionaryCollection_WhenHaveClassWithClassField()
        {
            var printer = ObjectPrinter.For<Dictionary<object, Person>>();
            var dictionary = new Dictionary<object, Person> { [2]=new Person { Age = 22, Name = "Sirgey", Car = new Vehicle("BMW", 230, 1500, 2003) } };
            printedString = printer.PrintToString(dictionary);
            printedString.Should().Contain("[[2] = Person; Name = Sirgey; Age = 22; HaveCar = True; Car = Vehicle; CarNumber = 0; BrandAuto = BMW; Power = 230; Weight = 1500; AgeOfCar = 19]");
        }
    }
}
