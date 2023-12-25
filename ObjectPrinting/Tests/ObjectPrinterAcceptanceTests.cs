using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private Person simplePerson;
        private Person personWithCyclingReference;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            simplePerson = new Person
            {
                Id = new Guid("0f8fad5b-d9cb-469f-a165-70867728950e"), Name = "simplePerson", Age = 45, Height = 180
            };
            personWithCyclingReference = new Person
                { Id = new Guid("b1754c14-d296-4b0f-a09a-030017f4461f"), Name = "Cycle", Age = 34, Height = 192 };
            personWithCyclingReference.Parents = new Person[] { personWithCyclingReference };
        }

        [Test]
        public void ObjectPrinter_Should_Serialize_All_Properties_When_Setting_Are_Default()
        {
            var printer = ObjectPrinter.For<Person>();
            var actual = printer.PrintToString(simplePerson);
            actual.Should()
                .Be(
                    $"Person\n\tId = 0f8fad5b-d9cb-469f-a165-70867728950e\n\tName = simplePerson\n\tHeight = 180\n\tAge = 45\n\tParents = null");
        }

        [Test]
        public void ObjectPrinter_Should_Excluding_PropertyType()
        {
            var printer = ObjectPrinter.For<Person>();
            var actual = printer.Exclude<int>().PrintToString(simplePerson);
            actual.Should()
                .Be(
                    "Person\n\tId = 0f8fad5b-d9cb-469f-a165-70867728950e\n\tName = simplePerson\n\tHeight = 180\n\tParents = null");
        }

        [Test]
        public void ObjectPrinter_Should_Excluding_Property()
        {
            var printer = ObjectPrinter.For<Person>();
            var actual = printer.Exclude(person => person.Name).PrintToString(simplePerson);
            actual.Should()
                .Be(
                    "Person\n\tId = 0f8fad5b-d9cb-469f-a165-70867728950e\n\tHeight = 180\n\tAge = 45\n\tParents = null");
        }

        [Test]
        public void ObjectPrinter_Should_Set_Custom_Serialize_For_Type()
        {
            var printer = ObjectPrinter.For<Person>();
            var actual = printer.SetCustomTypeSerializer<int>(i => 0.ToString()).PrintToString(simplePerson);
            actual.Should().Be("");
        }

        [Test]
        public void ObjectPrinter_Should_Set_Custom_Serializer_For_Property()
        {
            var printer = ObjectPrinter.For<Person>();
            var actual = printer.SerializeByProperty(person => person.Height).SetSerializer(height => height * 10 + "")
                .PrintToString(simplePerson);
            actual.Should()
                .Be(
                    "Person\n\tId = 0f8fad5b-d9cb-469f-a165-70867728950e\n\tName = simplePerson\n\tHeight = 1800\n\tAge = 45\n\tParents = null");
        }

        [Test]
        public void ObjectPrinter_Should_Trimm_String_Properties()
        {
            var printer = ObjectPrinter.For<Person>();
            var actual = printer.SerializeByProperty(person => person.Name).TrimmedStringProperty(2)
                .PrintToString(simplePerson);
            actual.Should()
                .Be(
                    "Person\n\tId = 0f8fad5b-d9cb-469f-a165-70867728950e\n\tName = si\n\tHeight = 180\n\tAge = 45\n\tParents = null");
        }

        [Test]
        public void ObjectPrinter_Should_Set_Culture_For_IFormatter_Types()
        {
            var d = 123456789.12345;
            var printer = ObjectPrinter.For<double>();
            var actual = printer.SetCulture<double>("", CultureInfo.InvariantCulture).PrintToString(d);
            actual.Should().Be("123456789,12345");
        }

        [Test]
        public void ObjectPrinter_Should_Serialize_IEnumerable_Types()
        {
            var list = new List<string>() { "hello", "world", "!" };
            var printer = ObjectPrinter.For<List<string>>();
            var actual = printer.PrintToString(list);
            actual.Should().Be("\n\thello\n\tworld\n\t!");
        }

        [Test]
        public void ObjectPrinter_Should_Serialize_IDictionary_Types()
        {
            var dict = new Dictionary<int, string>();
            dict[1] = "hello";
            dict[2] = "world";
            dict[3] = "!";

            var printer = ObjectPrinter.For<Dictionary<int, string>>();
            var actual = printer.PrintToString(dict);
            actual.Should().Be("\n\t1 = hello\n\t2 = world\n\t3 = !");
        }

        [Test]
        public void ObjectPrinter_Should_Ignore_Cycling_Reference()
        {
            var printer = ObjectPrinter.For<Person>();
            var actual = printer.PrintToString(personWithCyclingReference);
            actual.Should()
                .Be(
                    "Person\n\tId = b1754c14-d296-4b0f-a09a-030017f4461f\n\tName = Cycle\n\tHeight = 192\n\tAge = 34\n\tParents = \n\t\t\tCycling references");
        }


        [Test]
        public void PrintToString_Extension_Can_Call_From_Object()
        {
            var actual = simplePerson.PrintToString();
            actual.Should()
                .Be(
                    $"Person\n\tId = 0f8fad5b-d9cb-469f-a165-70867728950e\n\tName = simplePerson\n\tHeight = 180\n\tAge = 45\n\tParents = null");
        }

        [Test]
        public void PrintToString_Extension_Can_Call_From_Object_With_Config()
        {
            var actual = simplePerson.PrintToString(person => person.Exclude(p => p.Name));
            actual.Should()
                .Be(
                    $"Person\n\tId = 0f8fad5b-d9cb-469f-a165-70867728950e\n\tHeight = 180\n\tAge = 45\n\tParents = null");
        }
    }
}