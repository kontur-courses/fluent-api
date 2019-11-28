using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Tests;

namespace ObjectPrinting_Test
{
    public class PrintingConfig_Tests
    {
        private Person testPerson;
        private PrintingConfig<Person> printer;

        [SetUp]
        public void SetUp()
        {
            testPerson = new Person {Name = "Phil", Age = 22, Height = 190};
            printer = ObjectPrinter.For<Person>();
        }
        
        [Test]
        public void ObjectPrinter_WithoutConfigs_ReturnRightLinesCount()
        {
            var result = printer.PrintToString(testPerson);
            var linesCount = result.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length;
            const int expectedLinesCount = 5;
            linesCount.Should().Be(expectedLinesCount);
        }

        [Test]
        public void ObjectPrinter_WithoutConfig_ReturnRightResult()
        {
            var result = printer.PrintToString(testPerson);
            var expectedResult = new StringBuilder()
                .Append("Person\r\n")
                .Append("\tId = Guid\r\n")
                .Append("\tName = Phil\r\n")
                .Append("\tHeight = 190\r\n")
                .Append("\tAge = 22\r\n")
                .ToString();
            result.Should().BeEquivalentTo(expectedResult);
        }
        
        [Test]
        public void ObjectPrinter_WithExcludingType_ReturnRightResult()
        {
            printer.Excluding<Guid>();
            var result = printer.PrintToString(testPerson);
            var expectedResult = new StringBuilder()
                .Append("Person\r\n")
                .Append("\tName = Phil\r\n")
                .Append("\tHeight = 190\r\n")
                .Append("\tAge = 22\r\n")
                .ToString();
            result.Should().BeEquivalentTo(expectedResult);
        }
        
        [Test]
        public void ObjectPrinter_WithAlternativeSerializationForType_ReturnRightResult()
        {
            printer.Printing<string>().Using(s => s.ToUpper());
            var result = printer.PrintToString(testPerson);
            var expectedResult = new StringBuilder()
                .Append("Person\r\n")
                .Append("\tId = Guid\r\n")
                .Append("\tName = PHIL\r\n")
                .Append("\tHeight = 190\r\n")
                .Append("\tAge = 22\r\n")
                .ToString();
            result.Should().BeEquivalentTo(expectedResult);
        }
        
        [Test]
        public void ObjectPrinter_WithAlternativeSerializationForProperty_ReturnRightResult()
        {
            printer.Printing(p => p.Height).Using(h => new StringBuilder().Append(h).Append(" см").ToString());
            var result = printer.PrintToString(testPerson);
            var expectedResult = new StringBuilder()
                .Append("Person\r\n")
                .Append("\tId = Guid\r\n")
                .Append("\tName = PHIL\r\n")
                .Append("\tHeight = 190 см\r\n")
                .Append("\tAge = 22\r\n")
                .ToString();
            result.Should().BeEquivalentTo(expectedResult);
        }
        
        [Test]
        public void ObjectPrinter_WithCutting_ReturnRightResult()
        {
            printer.Printing(p => p.Name).CutToLength(1);
            var result = printer.PrintToString(testPerson);
            var expectedResult = new StringBuilder()
                .Append("Person\r\n")
                .Append("\tId = Guid\r\n")
                .Append("\tName = P\r\n")
                .Append("\tHeight = 190\r\n")
                .Append("\tAge = 22\r\n")
                .ToString();
            result.Should().BeEquivalentTo(expectedResult);
        }
        
        [Test]
        public void ObjectPrinter_WithExcludingProperty_ReturnRightResult()
        {
            printer.Excluding(p => p.Height);
            var result = printer.PrintToString(testPerson);
            var expectedResult = new StringBuilder()
                .Append("Person\r\n")
                .Append("\tId = Guid\r\n")
                .Append("\tName = Phil\r\n")
                .Append("\tAge = 22\r\n")
                .ToString();
            result.Should().BeEquivalentTo(expectedResult);
        }
        
        [Test]
        public void ObjectPrinter_WithDefaultSettings_ReturnRightResult()
        {
            var result = testPerson.PrintToString();
            var expectedResult = new StringBuilder()
                .Append("Person\r\n")
                .Append("\tId = Guid\r\n")
                .Append("\tName = Phil\r\n")
                .Append("\tHeight = 190\r\n")
                .Append("\tAge = 22\r\n")
                .ToString();
            result.Should().BeEquivalentTo(expectedResult);
        }
        
        [Test]
        public void ObjectPrinter_WithDefaultSettingsWithConfig_ReturnRightResult()
        {
            var result = testPerson.PrintToString(s => s.Excluding<Guid>());
            var expectedResult = new StringBuilder()
                .Append("Person\r\n")
                .Append("\tName = Phil\r\n")
                .Append("\tHeight = 190\r\n")
                .Append("\tAge = 22\r\n")
                .ToString();
            result.Should().BeEquivalentTo(expectedResult);
        }
        
        [Test]
        public void ObjectPrinter_CollectionPrinting_ReturnRightResult()
        {
            var testDictionary = new Dictionary<string, string>{{"a", "a1"}, {"b", "b1"}, {"c", "c1"}};
            var result = testDictionary.PrintToString();
            var expectedResult = new StringBuilder()
                .Append("Dictionary<String, String>\r\n")
                .Append("\tKeyValuePair<String, String>\r\n")
                .Append("\t\tKey = a\r\n")
                .Append("\t\tValue = a1\r\n")
                .Append("\tKeyValuePair<String, String>\r\n")
                .Append("\t\tKey = b\r\n")
                .Append("\t\tValue = b1\r\n")
                .Append("\tKeyValuePair<String, String>\r\n")
                .Append("\t\tKey = c\r\n")
                .Append("\t\tValue = c1\r\n")
                .ToString();
            result.Should().BeEquivalentTo(expectedResult);
        }
        
        [Test]
        public void ObjectPrinter_CyclingLinks_ReturnRightResult()
        {
            var firstPerson = new Child(){Name = "Pit"};
            var secondPerson = new Child(){Name = "Rik"};
            firstPerson.Brother = secondPerson;
            secondPerson.Brother = firstPerson;
            var result = firstPerson.PrintToString();
            var expectedResult = new StringBuilder()
                .Append("Child\r\n")
                .Append("\tName = Pit\r\n")
                .Append("\tBrother = Child\r\n")
                .Append("\t\tName = Rik\r\n")
                .Append("\t\tBrother = Child\r\n")
                .ToString();
            result.Should().BeEquivalentTo(expectedResult);
        }
    }
}