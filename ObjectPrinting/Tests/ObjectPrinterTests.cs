using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    class ObjectPrinterTests
    {
        [TestCase(5, TestName = "When object is int")]
        [TestCase("abc", TestName = "When object is string")]
        public void PrintToString_OnDefaultSettings(object obj)
        {
            var expected = $"{obj} (Hash: {obj.GetHashCode()})";
            ObjectPrinter.For<object>().PrintToString(obj).Should().Be(expected);
        }

        [Test]
        public void PrintToString_OnIEnumerable()
        {
            var array = new[] {12, 15, 20} as IEnumerable<int>;
            var expected = $"{{\r\n\t0: 12 (Hash: 12)\r\n\t1: 15 (Hash: 15)\r\n\t2: 20 (Hash: 20)\r\n}} (Hash: {array.GetHashCode()})";
            array.PrintToString().Should().Be(expected);
        }

        [Test]
        public void PrintToString_OnIDictionary()
        {
            var dict = new Dictionary<string, Node> {{"abc", new Node()}} as IDictionary<string, Node>;
            var kvPair = dict.First();
            var expected =
                $"{{\r\n\t{{\r\n\t\tkey: abc (Hash: {kvPair.Key.GetHashCode()})\r\n\t\tvalue: " +
                $"Node (Hash: {kvPair.Value.GetHashCode()})\r\n\t\t\t\tneighbor = null\r\n\t}}\r\n}} (Hash: {dict.GetHashCode()})";

            dict.PrintToString().Should().Be(expected);
        }

        [Test]
        public void PrintToString_ExcludingIntProperties()
        {
            var person = new Person();
            var expected =
                $"Person (Hash: {person.GetHashCode()})\r\n\tId = Guid (Hash: 0)\r\n\r\n\tName = null\r\n\tSurname = null\r\n\tHeight = 0 (Hash: 0)\r\n";
            ObjectPrinter.For<Person>().Excluding<int>().PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_UsingNonDefaultCulture()
        {
            var culture = CultureInfo.InvariantCulture;
            var number = 58.96d;

            var expected = $"{number.ToString(culture)} (Hash: {number.GetHashCode()})";

            var result = ObjectPrinter.For<double>().UsingNumbersCulture(CultureInfo.InvariantCulture).PrintToString(number);
        }

        [Test]
        public void ExtensionForObjects_IsWorking()
        {
            42.Serialize().PrintToString().Should().Be("42 (Hash: 42)");
        }

        [Test]
        public void ObjectPrinter_ExcludingProperty()
        {
            var person = new Person();
            var expected =
                $"Person (Hash: {person.GetHashCode()})\r\n\tId = Guid (Hash: 0)\r\n\r\n\tName = " +
                "null\r\n\tSurname = null\r\n\tHeight = 0 (Hash: 0)\r\n\tAge = 0 (Hash: 0)\r\n";

            var result = person.Serialize().Excluding(p => p.FriendsCount).PrintToString().Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_TrimmingStringType()
        {
            var person = new Person {Name = "Abracadabra", Surname = "Abracadabra"};
            var expected = $"Person (Hash: {person.GetHashCode()})\r\n\tName = Abrac (Hash: " +
                           $"{person.Name.GetHashCode()})\r\n\tSurname = Abrac (Hash: " +
                           $"{person.Surname.GetHashCode()})\r\n";

            person.Serialize().Serializing<string>().TrimmingToLength(5).Excluding<int>().Excluding<Guid>()
                .Excluding<double>().PrintToString().Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_TrimmingStringProperty()
        {
            var person = new Person {Name = "Abracadabra", Surname = "Abracadabra"};
            var expected = $"Person (Hash: {person.GetHashCode()})\r\n\tId = Guid (Hash: 0)\r\n\r\n\tName = Abrac " +
                           $"(Hash: {person.Name.GetHashCode()})\r\n\tSurname = Abracadabra (Hash: {person.Surname.GetHashCode()})\r\n\tHeight " +
                           "= 0 (Hash: 0)\r\n\tAge = 0 (Hash: 0)\r\n\tFriendsCount = 0 (Hash: 0)\r\n";

            person.Serialize().Serializing(p => p.Name).TrimmingToLength(5).PrintToString().Should().Be(expected);
        }
    }
}
