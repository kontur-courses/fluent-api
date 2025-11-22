using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests : ObjectPrinterTestsSetup
    {

        [Test]
        public void PrintToString_ReturnsCorrectFormat_WithSimpleObject()
        {
            var expected = "Person" + Environment.NewLine +
                           "\tName = John Doe" + Environment.NewLine +
                           "\tHeight = 180.5" + Environment.NewLine +
                           "\tBirthDate = " + TestPerson.BirthDate + Environment.NewLine +
                           "\tAge = 30" + Environment.NewLine;

            var result = PersonConfig.PrintToString(TestPerson);

            Assert.That(expected, Is.EqualTo(result));
        }

        [Test]
        public void PrintToString_ReturnsNullString_WithNullObject()
        {
            var result = PersonConfig.PrintToString(null);

            Assert.That(result, Is.EqualTo("null" + Environment.NewLine));
        }

        [Test]
        public void PrintToString_DetectsAndHandlesCyclicReference_WhenObjectsReferenceEachOther()
        {
            var node1 = new Node { Name = "Node1" };
            var node2 = new Node { Name = "Node2" };

            node1.Next = node2;
            node2.Next = node1;

            var result = NodeConfig.PrintToString(node1);

            Assert.That(result, Contains.Substring("Cyclic reference detected"));
        }

        [Test]
        public void PrintToString_WorksCorrectly_WhenMultipleConfigurationsAreCombined()
        {
            var config = new PrintingConfig<Person>();
            config.Exclude(p => p.Height)
                .SetSerialization(p => p.Name, name => name.ToUpper())
                .SetSerialization<int>(i => $"{i} years old")
                .TrimStringsTo(7);

            var expected = "Person" + Environment.NewLine +
                           "\tName = JOHN DO" + Environment.NewLine +
                           $"\tBirthDate = {TestPerson.BirthDate}" + Environment.NewLine +
                           "\tAge = 30 years old" + Environment.NewLine;

            var result = config.PrintToString(TestPerson);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Exclude_ExcludesProperties_WhenTypeIsSpecified()
        {
            PersonConfig.Exclude<int>();

            var expected = "Person" + Environment.NewLine +
                           "\tName = John Doe" + Environment.NewLine +
                           "\tHeight = 180.5" + Environment.NewLine +
                           $"\tBirthDate = {TestPerson.BirthDate}" + Environment.NewLine;

            var result = PersonConfig.PrintToString(TestPerson);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Exclude_ExcludesAllProperties_WhenMultipleTypesAreSpecified()
        {
            PersonConfig.Exclude<int>()
                .Exclude<double>();

            var expected = "Person" + Environment.NewLine +
                           "\tName = John Doe" + Environment.NewLine +
                           $"\tBirthDate = {TestPerson.BirthDate}" + Environment.NewLine;

            var result = PersonConfig.PrintToString(TestPerson);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void SetSerialization_UsesCustomSerializer_WhenTypeIsSpecified()
        {
            PersonConfig.SetSerialization<int>(i => $"Integer: {i}");

            var expected = "Person" + Environment.NewLine +
                           "\tName = John Doe" + Environment.NewLine +
                           "\tHeight = 180.5" + Environment.NewLine +
                           "\tBirthDate = " + TestPerson.BirthDate + Environment.NewLine +
                           "\tAge = Integer: 30" + Environment.NewLine;

            var result = PersonConfig.PrintToString(TestPerson);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void SetSerialization_FormatsNumbersWithCustomLogic_WhenDoubleTypeIsSpecified()
        {
            PersonConfig.SetSerialization<double>(d => $"{d} centimeters");

            var expected = "Person" + Environment.NewLine +
                           "\tName = John Doe" + Environment.NewLine +
                           "\tHeight = 180.5 centimeters" + Environment.NewLine +
                           "\tBirthDate = " + TestPerson.BirthDate + Environment.NewLine +
                           "\tAge = 30" + Environment.NewLine;

            var result = PersonConfig.PrintToString(TestPerson);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void SetSerialization_UsesCustomSerializer_WhenPropertyIsSpecified()
        {
            PersonConfig.SetSerialization(p => p.Name, name => $"Name: {name.ToUpper()}");

            var expected = "Person" + Environment.NewLine +
                           "\tName = Name: JOHN DOE" + Environment.NewLine +
                           "\tHeight = 180.5" + Environment.NewLine +
                           "\tBirthDate = " + TestPerson.BirthDate + Environment.NewLine +
                           "\tAge = 30" + Environment.NewLine;

            var result = PersonConfig.PrintToString(TestPerson);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void SetCulture_UsesSpecifiedCulture_WhenDoubleTypeIsSpecified()
        {
            PersonConfig.SetCulture<double>(new CultureInfo("de-DE"));

            var expected = "Person" + Environment.NewLine +
                           "\tName = John Doe" + Environment.NewLine +
                           "\tHeight = 180,5" + Environment.NewLine +
                           "\tBirthDate = " + TestPerson.BirthDate + Environment.NewLine +
                           "\tAge = 30" + Environment.NewLine;

            var result = PersonConfig.PrintToString(TestPerson);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void SetCulture_UsesSpecifiedCulture_WhenDecimalTypeIsSpecified()
        {
            CompanyConfig.SetCulture<decimal>(new CultureInfo("fr-FR"));

            var expected = "Company" + Environment.NewLine +
                           "\tName = Test Company" + Environment.NewLine +
                           "\tEmployees = List" + Environment.NewLine +
                           "\t\t[0] = Person" + Environment.NewLine +
                           "\t\t\tName = John Doe" + Environment.NewLine +
                           "\t\t\tHeight = 180.5" + Environment.NewLine +
                           "\t\t\tBirthDate = " + TestPerson.BirthDate + Environment.NewLine +
                           "\t\t\tAge = 30" + Environment.NewLine +
                           "\tBudget = 1000000,75" + Environment.NewLine;

            var result = CompanyConfig.PrintToString(TestCompany);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void SetCulture_UsesSpecifiedCulture_WhenDateTimeTypeIsSpecified()
        {
            var culture = new CultureInfo("en-US");
            PersonConfig.SetCulture<DateTime>(culture);

            var expected = "Person" + Environment.NewLine +
                           "\tName = John Doe" + Environment.NewLine +
                           "\tHeight = 180.5" + Environment.NewLine +
                           "\tBirthDate = " + TestPerson.BirthDate.ToString(culture) + Environment.NewLine +
                           "\tAge = 30" + Environment.NewLine;

            var result = PersonConfig.PrintToString(TestPerson);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void SetCulture_ThrowsArgumentException_WhenCharTypeIsSpecified()
        {
            Assert.Throws<ArgumentException>(() =>
                PersonConfig.SetCulture<char>(CultureInfo.CurrentCulture));
        }

        [Test]
        public void TrimStringsTo_TrimsLongStrings_WhenValidLengthIsSpecified()
        {
            PersonConfig.TrimStringsTo(5);

            var expected = "Person" + Environment.NewLine +
                           "\tName = John " + Environment.NewLine +
                           "\tHeight = 180.5" + Environment.NewLine +
                           "\tBirthDate = " + TestPerson.BirthDate + Environment.NewLine +
                           "\tAge = 30" + Environment.NewLine;

            var result = PersonConfig.PrintToString(TestPerson);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void TrimStringsTo_ThrowsArgumentException_WhenNegativeLengthIsSpecified()
        {
            Assert.Throws<ArgumentException>(() => PersonConfig.TrimStringsTo(-1));
        }

        [Test]
        public void Exclude_ExcludesSpecificProperty_WhenPropertyIsSpecified()
        {
            PersonConfig.Exclude(p => p.Age);

            var expected = "Person" + Environment.NewLine +
                           "\tName = John Doe" + Environment.NewLine +
                           "\tHeight = 180.5" + Environment.NewLine +
                           "\tBirthDate = " + TestPerson.BirthDate + Environment.NewLine;

            var result = PersonConfig.PrintToString(TestPerson);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void PrintToString_SerializesCorrectly_WhenArrayIsPassed()
        {
            var array = new[] { 1, 2, 3, 4, 5 };
            var config = new PrintingConfig<int[]>();

            var expected = "Array" + Environment.NewLine +
                           "\t[0] = 1" + Environment.NewLine +
                           "\t[1] = 2" + Environment.NewLine +
                           "\t[2] = 3" + Environment.NewLine +
                           "\t[3] = 4" + Environment.NewLine +
                           "\t[4] = 5" + Environment.NewLine;

            var result = config.PrintToString(array);


            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void PrintToString_SerializesCorrectly_WhenListIsPassed()
        {
            var list = new List<string> { "apple", "banana", "cherry" };
            var config = new PrintingConfig<List<string>>();

            var expected = "List" + Environment.NewLine +
                           "\t[0] = apple" + Environment.NewLine +
                           "\t[1] = banana" + Environment.NewLine +
                           "\t[2] = cherry" + Environment.NewLine;

            var result = config.PrintToString(list);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void PrintToString_SerializesCorrectly_WhenDictionaryIsPassed()
        {
            var dict = new Dictionary<string, int>
            {
                ["one"] = 1,
                ["two"] = 2,
                ["three"] = 3
            };
            var config = new PrintingConfig<Dictionary<string, int>>();

            var expected = "Dictionary" + Environment.NewLine +
                           "\t[one] = 1" + Environment.NewLine +
                           "\t[two] = 2" + Environment.NewLine +
                           "\t[three] = 3" + Environment.NewLine;

            var result = config.PrintToString(dict);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void PrintToString_SerializesCorrectly_WhenStackIsPassed()
        {
            var stack = new Stack<string>();
            var config = new PrintingConfig<Stack<string>>();

            var expected = "Collection" + Environment.NewLine;

            var result = config.PrintToString(stack);

            Assert.That(result, Is.EqualTo(expected));
        }
    }
}