using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using static System.Environment;

namespace ObjectPrinterTests
{
    public class ObjectPrinterTests
    {
        private static PrintingConfig<Person> printer;

        private static Person Xsitin;

        [SetUp]
        public void SetUp()
        {
            Xsitin = new Person
                {Name = "xsitin", Age = 19, Height = 185.1, Id = new Guid("dddddddddddddddddddddddddddddddd")};
            printer = ObjectPrinter.For<Person>();
        }

        [Test]
        public void ObjectPrinter_null_StingWithNull()
        {
            printer.PrintToString(null).Should().Be("null");
        }

        [Test]
        public void ObjectPrinter_EmptyPerson_EmptyValuesInString()
        {
            printer.PrintToString(new Person()).Should().Be(
                string.Join(NewLine + '\t', "Person", "Id = 00000000-0000-0000-0000-000000000000",
                    "Name = null", "Height = 0", "Age = 0", "otherPerson = null"));
        }

        [Test]
        public void ObjectPrinter_ExcludeHeight_DefaultPersonWithoutHeight()
        {
            printer.Excluding(person => person.Height).PrintToString(new Person()).Should().Be(
                string.Join(NewLine + '\t', "Person", "Id = 00000000-0000-0000-0000-000000000000",
                    "Name = null", "Age = 0", "otherPerson = null"));
        }

        [Test]
        public void ObjectPrinter_AnotherSerializationForInt_BinaryInt()
        {
            printer.Printing<int>().Using(x => Convert.ToString(x, 2));
            printer.PrintToString(Xsitin).Should().Be(
                string.Join(NewLine + '\t', "Person",
                    "Id = dddddddd-dddd-dddd-dddd-dddddddddddd", "Name = xsitin",
                    "Height = 185,1", "Age = 10011", "otherPerson = null"));
        }

        [Test]
        public void ObjectPrinter_ExcludeInt_PersonWithoutAge()
        {
            printer.Excluding<int>().PrintToString(Xsitin).Should().Be(string.Join(NewLine + '\t', "Person",
                "Id = dddddddd-dddd-dddd-dddd-dddddddddddd", "Name = xsitin",
                "Height = 185,1", "otherPerson = null"));
        }

        [Test]
        public void ObjectPrinter_SetInvariantCultureForDouble_HeightWithDot()
        {
            printer.Printing<double>().Using(CultureInfo.InvariantCulture).PrintToString(Xsitin).Should().Be(
                string.Join(
                    NewLine + '\t', "Person",
                    "Id = dddddddd-dddd-dddd-dddd-dddddddddddd", "Name = xsitin",
                    "Height = 185.1", "Age = 19", "otherPerson = null"));
        }

        [Test]
        public void ObjectPrinter_TrimName_InNameOnlyFirstLetter()
        {
            printer.Printing(x => x.Name).TrimmedToLength(1);
            printer.PrintToString(Xsitin).Should().Be(string.Join(
                NewLine + '\t', "Person",
                "Id = dddddddd-dddd-dddd-dddd-dddddddddddd", "Name = x",
                "Height = 185,1", "Age = 19", "otherPerson = null"));
        }

        [Test]
        public void ObjectPrinter_ObjectWithCyclicLink_ThrowsInternalBufferOverflowException()
        {
            var other = new Person();
            Xsitin.otherPerson = other;
            other.otherPerson = Xsitin;
            Assert.Catch<InternalBufferOverflowException>(() => printer.PrintToString(Xsitin));
        }

        [Test]
        public void ObjectPrinter_ObjectWithNesting_CorrectSerialization()
        {
            Xsitin.otherPerson = new Person();
            Xsitin.PrintToString().Should().Be(string.Join($"{NewLine}\t", "Person",
                "Id = dddddddd-dddd-dddd-dddd-dddddddddddd", "Name = xsitin",
                "Height = 185,1", "Age = 19", "otherPerson = " + string.Join($"{NewLine}\t\t", "Person",
                    "Id = 00000000-0000-0000-0000-000000000000",
                    "Name = null", "Height = 0", "Age = 0", "otherPerson = null")));
        }

        [Test]
        public void ObjectPrinter_CustomWayTypeSerialization_CorrectSerialization()
        {
            "something".PrintToString(x => x.Printing<string>().Using(y => "xsitin")).Should().Be("xsitin");
            new Person().PrintToString(x => x.Printing<Person>().Using(x => "person")).Should().Be("person");
        }

        [Test]
        public void ObjectPrinter_CustomPropertySerialization_CorrectSerialization()
        {
            new Person().PrintToString(x => x.Printing(y => y.Name).Using(n => "xsitin")).Should().Be(string.Join(
                NewLine + '\t', "Person", "Id = 00000000-0000-0000-0000-000000000000",
                "Name = xsitin", "Height = 0", "Age = 0", "otherPerson = null"));
        }

        [Test]
        public void ObjectPrinter_IncorrectChoiceOfProperty_ThrowsException()
        {
            Assert.Catch(() => Xsitin.PrintToString(x => x.Printing(p => "").Using(s => "xsitin")));
        }

        [Test]
        public void ObjectPrinter_DictionarySerialization_CorrectSerialization()
        {
            var dic = new Dictionary<int, Dictionary<int, string>>
            {
                [1] = new Dictionary<int, string> {[2] = "1to2", [3] = "1to3"}
            };
            dic.PrintToString().Should()
                .Be(
                    $"Dictionary`2{NewLine}\tKeyValuePair`2{NewLine}\t\tKey = 1{NewLine}\t\tValue = Dictionary`2{NewLine}" +
                    $"\t\t\tKeyValuePair`2{NewLine}\t\t\t\tKey = 2{NewLine}\t\t\t\tValue = 1to2{NewLine}\t\t\t" +
                    $"KeyValuePair`2{NewLine}\t\t\t\tKey = 3{NewLine}\t\t\t\tValue = 1to3");
        }

        [Test]
        public void ObjectPrinter_ListSerialization_CorrectSerialization()
        {
            var list = new List<string> {"first", "second"};
            list.PrintToString().Should().Be($"List`1{NewLine}\tfirst{NewLine}\tsecond");
        }

        [Test]
        public void ObjectPrinter_ArraySerialization_CorrectSerialization()
        {
            var array = new[] { "first","second"};
            array.PrintToString().Should().Be($"String[]{NewLine}\tfirst{NewLine}\tsecond");
        }
    }
}