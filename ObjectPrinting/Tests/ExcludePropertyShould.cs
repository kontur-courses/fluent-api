using NUnit.Framework.Interfaces;
using NUnit.Framework;
using ObjectPrinting.Tests.TestClasses;
using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ExcludePropertyShould
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
        public void PrintPersonWithoutIdProperty_WhenExcludeIdField()
        {
            printer = printer.Excluding(x=>x.Id);
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().NotContain("Id");
        }
        [Test]
        public void PrintPersonWithoutNameProperty_WhenExcludeNameField()
        {
            
            printer = printer.Excluding(x=>x.Name);
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().NotContain("Name");
        }
        [Test]
        public void PrintPersonWithoutHeightProperty_WhenExcludeHeightField()
        {
            printer = printer.Excluding(x=>x.Height);
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().NotContain("Height");
        }
        [Test]
        public void PrintPersonWithoutHaveCarProperty_WhenExcludeHaveCarField()
        {
            printer = printer.Excluding(x=>x.HaveCar);
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().NotContain("HaveCar");
        }
        [Test]
        public void PrintPersonWithoutCarBrandProperty_WhenExcludeHaveCarBrandField()
        {
            printer = printer.Excluding(x=>x.Car.BrandAuto);
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().NotContain("BrandAuto");
        }

        [Test]
        public void PrintPersonWithoutTypeListProperty_WhenExcludeTypeListField()
        {
            printer = printer.Excluding(x=>x.TypeList);
            simplePerson.TypeList = new List<int> { 1, 2, 3 };
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().NotContain("TypeList");

        }
        [Test]
        public void PrintPersonWithoutTypeDictProperty_WhenExcludeTypeDictField()
        {
            printer = printer.Excluding(x=>x.TypeDict);
            simplePerson.TypeDict = new Dictionary<int, object>() { [1] = "Car", [2] = 2123.3 };
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().NotContain("TypeDict");
        }
        [Test]
        public void PrintPersonWithoutTypeSetProperty_WhenExcludeTypeSetField()
        {
            printer = printer.Excluding(x=>x.TypeSet);
            simplePerson.TypeSet = new HashSet<string> { "something text", "for hashset test" };
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().NotContain("TypeSet");
        }
        [Test]
        public void PrintPersonWithoutTypeArrayProperty_WhenExcludeTypeArrayField()
        {
            printer = printer.Excluding(x=>x.TypeArray);
            simplePerson.TypeArray = new int[] { 1, 2, 3 };
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().NotContain("TypeArray");
        }
        [Test]
        public void PrintPersonWithoutParentProperty_WhenExcludeSelfTypeField()
        {
            printer = printer.Excluding(x=>x.Parent);
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().NotContain("Parent");
        }
    }
}
