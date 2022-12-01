using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Tests;

namespace ObjectPrintingTests
{
    [TestFixture]
    internal class ObjectPrinterTests
    {
        private Person person;
        private string newLine;

        [SetUp]
        public void SetUp()
        {
            person = new Person() { Name = "Nikita", Age = 21, Height = 178, Id = Guid.NewGuid() };
            newLine = Environment.NewLine;
        }

        [Test]
        public void ObjectPrinter_ShouldPrintingOnlyNonExcludingTypes_WhenExcludedOneType()
        {
            var printer = ObjectPrinter.For<Person>().ExcludingType<Guid>();
            var result = printer.PrintToString(person);

            result.Should().Be($"Person{newLine}\tName = {person.Name}{newLine}" +
                               $"\tHeight = {person.Height}{newLine}\tAge = {person.Age}{newLine}");
        }

        [Test]
        public void ObjectPrinter_ShouldPrintingOnlyObjectType_WhenExcludedAllTypes()
        {
            var printer = ObjectPrinter.For<Person>()
                .ExcludingTypes(new Type[] { typeof(int), typeof(double), typeof(string), typeof(Guid) });
            var result = printer.PrintToString(person);

            result.Should().Be($"Person{newLine}");
        }

        [Test]
        public void ObjectPrinter_PrintPersonWithoutOneProperty_WhenExcludedOneProperty()
        {
            var printer = ObjectPrinter.For<Person>().ExcludingProperty(p => p.Id);
            var result = printer.PrintToString(person);

            result.Should().Be($"Person{newLine}\tName = {person.Name}{newLine}" +
                               $"\tHeight = {person.Height}{newLine}\tAge = {person.Age}{newLine}");
        }

        [Test]
        public void ObjectPrinter_PrintPersonWithAlternativeSerialization_WhenAlternativeSerializationWasSpecifiedForType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<string>()
                .With(s => $"AlternativeSerialization: {s}");
            var result = printer.PrintToString(person);

            result.Should().Be($"Person{newLine}\tId = {person.Id}{newLine}\tName = AlternativeSerialization: " +
                               $"{person.Name}{newLine}\tHeight = {person.Height}{newLine}\tAge = {person.Age}{newLine}");
        }

        [Test]
        public void ObjectPrinter_PrintPersonWithAlternativeSerialization_WhenAlternativeSerializationWasSpecifiedForProperty()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Id)
                .With(guid => $"Alternative serialization: {guid}");
            var result = printer.PrintToString(person);

            result.Should().Be($"Person{newLine}\tId = Alternative serialization: {person.Id}{newLine}\tName = " +
                               $"{person.Name}{newLine}\tHeight = {person.Height}{newLine}\tAge = {person.Age}{newLine}");
        }

        [Test]
        public void ObjectPrinter_PrintPersonWithAlternativeSerialization_WhenAlternativeSerializationWasSpecifiedForThreeProperty()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Id)
                .With(guid => $"Alternative 1: {guid}")
                .Printing(p => p.Name)
                .With(name => $"Alternative 2: {name}")
                .Printing(p => p.Age)
                .With(age => $"Alternative 3: {age}");

            var result = printer.PrintToString(person);

            result.Should().Be($"Person{newLine}\tId = Alternative 1: {person.Id}{newLine}" +
                               $"\tName = Alternative 2: {person.Name}{newLine}\tHeight = {person.Height}{newLine}" +
                               $"\tAge = Alternative 3: {person.Age}{newLine}");
        }

        [Test]
        public void ObjectPrinter_PrintPersonWithAlternativeSerialization_WhenAlternativeSerializationWasSpecifiedForTypeAndProperty()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Id)
                .With(guid => $"Alternative property: {guid}")
                .Printing<string>()
                .With(s => $"Alternative type: {s}");

            var result = printer.PrintToString(person);

            result.Should().Be($"Person{newLine}\tId = Alternative property: {person.Id}{newLine}" +
                               $"\tName = Alternative type: {person.Name}{newLine}\tHeight = {person.Height}{newLine}" +
                               $"\tAge = {person.Age}{newLine}");
        }

        [Test]
        public void ObjectPrinter_PrintPersonWithSpecialCulture_WhenSpecialCultureWasSpecifiedForType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>()
                .With(CultureInfo.CurrentCulture);
            var result = printer.PrintToString(person);

            result.Should().Be(
                $"Person{newLine}" + $"\tId = {person.Id}{newLine}\tName = {person.Name}{newLine}" +
                $"\tHeight = {person.Height.ToString(null, CultureInfo.CurrentCulture)}{newLine}" +
                $"\tAge = {person.Age}{newLine}");
        }

        [Test]
        public void ObjectPrinter_PrintPersonWithTrimsString_WhenTrimWasSpecifiedForStringType()
        {
            var maxLength = 1;
            var printer = ObjectPrinter.For<Person>()
                .Printing<string>()
                .Trim(maxLength);
            var result = printer.PrintToString(person);

            result.Should().Be(
                $"Person{newLine}" + $"\tId = {person.Id}{newLine}" +
                $"\tName = {person.Name.Substring(0, maxLength)}{newLine}" +
                $"\tHeight = {person.Height}{newLine}" +
                $"\tAge = {person.Age}{newLine}");
        }

        [Test]
        public void ObjectPrinter_PrintPersonWithTrimsString_WhenTrimWasSpecifiedForStringProperty()
        {
            var maxLength = 5;
            var printer = ObjectPrinter.For<Person>()
                .Printing(prop => prop.Name)
                .Trim(maxLength);
            var result = printer.PrintToString(person);

            result.Should().Be(
                $"Person{newLine}" + $"\tId = {person.Id}{newLine}" +
                $"\tName = {person.Name.Substring(0, maxLength)}{newLine}" +
                $"\tHeight = {person.Height}{newLine}" +
                $"\tAge = {person.Age}{newLine}");
        }

        [Test]
        public void ObjectPrinter_PrintPersonWithDefaultSerializationConfiguration_WhenExtensionMethodWasCalled()
        {
            var result = person.PrintToString();

            result.Should().Be(
                $"Person{newLine}" + $"\tId = {person.Id}{newLine}" +
                $"\tName = {person.Name}{newLine}" +
                $"\tHeight = {person.Height}{newLine}" +
                $"\tAge = {person.Age}{newLine}");
        }

        [Test]
        public void ObjectPrinter_PrintPersonWithSpecialSerializationConfiguration_WhenExtensionMethodWasCalled()
        {
            var result = person.PrintToString(conf => conf
                .ExcludingType<double>()
                .Printing<int>()
                .With(s => $"{s} - int"));

            result.Should().Be(
                $"Person{newLine}" + $"\tId = {person.Id}{newLine}" +
                $"\tName = {person.Name}{newLine}" +
                $"\tAge = {person.Age} - int{newLine}");
        }

        [Test]
        public void ObjectPrinter_ShouldPrintWithoutLoopback()
        {
            var house1 = new House();
            var house2 = new House() { RightAdjacentBuilding = house1, LeftAdjacentBuilding = house1 };

            var result = house2.PrintToString();
            result.Should().NotContain("Loopback!");
        }

        [Test]
        public void ObjectPrinter_CorrectPrintNode_WhenHasDifficultLoopback()
        {
            var house1 = new House { Address = "Street1" };
            var house2 = new House { Address = "Street2", LeftAdjacentBuilding = house1 };
            var house3 = new House { Address = "Street3", LeftAdjacentBuilding = house2 };
            var house4 = new House { Address = "Street4", LeftAdjacentBuilding = house3 };
            var house5 = new House { Address = "Street5", LeftAdjacentBuilding = house4 };

            house1.LeftAdjacentBuilding = house5;

            var result = house1.PrintToString(
                conf => conf.ExcludingProperty(house => house.RightAdjacentBuilding));

            result.Should().Be(
                $"{house1.GetType().Name}{newLine}" +
                $"\tAddress = {house1.Address}{newLine}" +
                $"\tLeftAdjacentBuilding = {house1.LeftAdjacentBuilding.GetType().Name}{newLine}" +
                $"\t\tAddress = {house5.Address}{newLine}" +
                $"\t\tLeftAdjacentBuilding = {house5.LeftAdjacentBuilding.GetType().Name}{newLine}" +
                $"\t\t\tAddress = {house4.Address}{newLine}" +
                $"\t\t\tLeftAdjacentBuilding = {house4.LeftAdjacentBuilding.GetType().Name}{newLine}" +
                $"\t\t\t\tAddress = {house3.Address}{newLine}" +
                $"\t\t\t\tLeftAdjacentBuilding = {house4.LeftAdjacentBuilding.GetType().Name}{newLine}" +
                $"\t\t\t\t\tAddress = {house2.Address}{newLine}" +
                $"\t\t\t\t\tLeftAdjacentBuilding = Loopback!{newLine}");
        }

        [Test]
        public void ObjectPrinter_ShouldPrintListProperty()
        {
            var vipHouse = new House { Address = "This SuperStreet" };
            var house = new House { Address = "This Street", LeftAdjacentBuilding = vipHouse };

            var street = new Street()
            {
                AddressBook = new Dictionary<Person, House>()
                {
                    { new Person { Name = "Name1", Age = 1 }, house },
                    { new Person { Name = "Name2", Age = 2 }, vipHouse }
                },
                Houses = new List<House>() { vipHouse, house },
                Id = Guid.NewGuid(),
                HonorablePersonsOfStreet = new[] { new Person() { Name = "Name3" } }
            };

            var printer = street.PrintToString(config => config
                .ExcludingType<Guid>()
                .ExcludingProperty(p => p.HonorablePersonsOfStreet)
                .ExcludingProperty(p => p.AddressBook));

            printer.Should().Be(
                $"{street.GetType().Name}{newLine}" +
                $"\tHouses = {street.Houses.GetType().Name}{newLine}" +
                $"\t\t0: House{newLine}" +
                $"\t\t\tAddress = This SuperStreet{newLine}" +
                $"\t\t\tLeftAdjacentBuilding = null{newLine}" +
                $"\t\t\tRightAdjacentBuilding = null{newLine}" +
                $"\t\t1: House{newLine}" +
                $"\t\t\tAddress = This Street{newLine}" +
                $"\t\t\tLeftAdjacentBuilding = House{newLine}" +
                $"\t\t\t\tAddress = This SuperStreet{newLine}" +
                $"\t\t\t\tLeftAdjacentBuilding = null{newLine}" +
                $"\t\t\t\tRightAdjacentBuilding = null{newLine}" +
                $"\t\t\tRightAdjacentBuilding = null{newLine}");
        }

        [Test]
        public void ObjectPrinter_ShouldPrintDictionaryProperty()
        {
            var vipHouse = new House { Address = "This SuperStreet" };
            var house = new House { Address = "This Street", LeftAdjacentBuilding = vipHouse };
            var person1 = new Person { Name = "Name1", Age = 1 };
            var person2 = new Person { Name = "Name2", Age = 2 };

            var street = new Street()
            {
                AddressBook = new Dictionary<Person, House>()
                {
                    { person1, null },
                    { person2, vipHouse }
                },
                Houses = new List<House>() { vipHouse, house },
                Id = Guid.NewGuid(),
                HonorablePersonsOfStreet = new[] { new Person() { Name = "Name3" } }
            };

            var printer = street.PrintToString(config => config
                .ExcludingType<Guid>()
                .ExcludingProperty(p => p.HonorablePersonsOfStreet)
                .ExcludingProperty(p => p.Houses));

            printer.Should().Be(
                $"{street.GetType().Name}{newLine}" +
                $"\tAddressBook = {street.AddressBook.GetType().Name}{newLine}" +
                $"\t\tObjectPrinting.Tests.Person: null{newLine}" +
                $"\t\tObjectPrinting.Tests.Person: House{newLine}" +
                $"\t\t\tAddress = This SuperStreet{newLine}" +
                $"\t\t\tLeftAdjacentBuilding = null{newLine}" +
                $"\t\t\tRightAdjacentBuilding = null{newLine}");
        }

        [Test]
        public void ObjectPrinter_ShouldPrintArrayProperty()
        {
            var vipHouse = new House { Address = "This SuperStreet" };
            var house = new House { Address = "This Street", LeftAdjacentBuilding = vipHouse };

            var street = new Street()
            {
                AddressBook = new Dictionary<Person, House>()
                {
                    { new Person { Name = "Name1", Age = 1 }, null },
                    { new Person { Name = "Name2", Age = 2 }, vipHouse }
                },
                Houses = new List<House>() { vipHouse, house },
                Id = Guid.NewGuid(),
                HonorablePersonsOfStreet = new[] { new Person() { Name = "Name3" } }
            };

            var printer = street.PrintToString(config => config
                .ExcludingType<Guid>()
                .ExcludingProperty(p => p.AddressBook)
                .ExcludingProperty(p => p.Houses));

            printer.Should().Be(
                $"{street.GetType().Name}{newLine}" +
                $"\tHonorablePersonsOfStreet = {street.HonorablePersonsOfStreet.GetType().Name}{newLine}" +
                $"\t\t0: Person{newLine}" +
                $"\t\t\tName = Name3{newLine}" +
                $"\t\t\tHeight = 0{newLine}" +
                $"\t\t\tAge = 0{newLine}");
        }
    }
}
