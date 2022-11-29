using NUnit.Framework;
using NUnit.Framework.Interfaces;
using ObjectPrinting.Tests.TestClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class CyclicReferenceShould
    {
        private string printedString;
        public Vehicle VehicleCar;
        private Person simplePerson;
        private Person childPerson;
        private PrintingConfig<Person> printer;
        
        [SetUp]
        public void SetUp()
        {
            printer = ObjectPrinter.For<Person>();
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
        public void PrintPersonWithoutExceptions_WhenChildPersonHaveReferenceOnParent()
        {
            simplePerson.Parent = childPerson;
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().Contain("Cyclic reference");
        }
        [Test]
        public void PrintPersonWithoutExceptions_WhenPersonHaveReferenceOnSelf()
        {
            simplePerson.Parent = simplePerson;
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().Contain("Cyclic reference");
        }
        [TestCase(10, TestName = "cyclic reference in iteration 10")]
        [TestCase(50, TestName = "cyclic reference in iteration 50")]
        [TestCase(100, TestName = "cyclic reference in iteration 100")]
        [TestCase(200, TestName = "cyclic reference in iteration 200")]
        public void PrintPersonWithoutExceptions_WhenPersonHaveManyIterationsWithoutCyclicReference(int countIteration)
        {
            var list= new List<Person>();
            for (var i = 0; i < countIteration; i++)
                list.Add(new Person(){Age=i});
            for (var i = 1; i < countIteration; i++)
                list[i].Parent = list[i - 1];
            list.First().Parent = list.LastOrDefault();
            printedString = printer.PrintToString(list.LastOrDefault());
            printedString.Should().Contain("Cyclic reference");
        }
    }
}
