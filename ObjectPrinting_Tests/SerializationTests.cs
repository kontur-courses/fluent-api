using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using FluentAssertions;
using ObjectPrinting;
using ObjectPrinting.Formatting;
using ObjectPrinting.PrintingConfigs;
using ObjectPrinting.Tests;
using Person = ObjectPrinting.Person;

namespace ObjectPrinting_Tests
{
    [TestFixture]
    public class SerializationTests
    {
        [Test]
        public void Should_SerializeInDefaultFormat_When_SerializeWithoutFormatSet()
        {
            const string expectedStr = "Person\n{\n\tId = Guid\n\tName = null\n\tHeight = 0\n\tAge = 0\n\tParent = null\n}";
            
            var result = new Person().PrintToString();
            
            result.Should().BeEquivalentTo(expectedStr);
        }
        
        [Test]
        public void Should_SerializeWithoutProperty_When_ExcludePropertyByName()
        {
            const string expectedStr = "Person\n{\n\tId = Guid\n\tHeight = 0\n\tAge = 0\n\tParent = null\n}";
            
            var result = new Person().PrintToString(x => x.Excluding(property => property.Name));
            
            result.Should().BeEquivalentTo(expectedStr);
        }
        
        [Test]
        public void Should_SerializeWithoutProperty_When_ExcludePropertyByType()
        {
            const string expectedStr = "Person\n{\n\tId = Guid\n\tName = null\n\tAge = 0\n\tParent = null\n}";
            
            var result = new Person().PrintToString(x => x.Excluding<double>());
            
            result.Should().BeEquivalentTo(expectedStr);
        }

        [Test]
        public void Should_SerializePropertyWithAlternativeMethod_When_SetSerializeMethodForType()
        {
            const string expectedStr = "Person\n{\n\tId = Guid\n\tName = null\n\tHeight = 0\n\tAge = TEST\n\tParent = null\n}";

            var result = new Person().PrintToString(x => x.Printing<int>().Using(variable => "TEST"));
            
            result.Should().BeEquivalentTo(expectedStr);
        }
        
        [Test]
        public void Should_SerializePropertyWithAlternativeMethod_When_SetSerializeMethodForSelectedProperty()
        {
            const string expectedStr = "Person\n{\n\tId = Guid\n\tName = TEST\n\tHeight = 0\n\tAge = 0\n\tParent = null\n}";

            var result = new Person().PrintToString(config => config.Printing(x => x.Name).Using(variable => "TEST"));
            
            result.Should().BeEquivalentTo(expectedStr);
        }
        
        [Test]
        public void Should_SerializeNumericPropertyWithCulture_When_SetCultureForNumericProperty()
        {
            const string expectedStr = "Person\n{\n\tId = Guid\n\tName = null\n\tHeight = 0\n\tAge = %55\n\tParent = null\n}";

            var myCulture = (CultureInfo)new CultureInfo("ru-RU").Clone();
            myCulture.NumberFormat.NegativeSign = "%";
            var person = new Person {Age = -55};
            
            var resultByName = person.PrintToString(config => config.Printing(x => x.Age).SetCulture(myCulture));
            var resultByTypes = person.PrintToString(config => config.Printing<int>().SetCulture(myCulture));
            
            resultByName.Should().BeEquivalentTo(expectedStr);
            resultByTypes.Should().BeEquivalentTo(expectedStr);
        }
        
        [Test]
        public void Should_SerializeStringPropertyWithStaticLength_When_SetSerializeStringPropertiesWithSelectedLength()
        {
            const string expectedStr = "Person\n{\n\tId = Guid\n\tName = HELLO\n\tHeight = 0\n\tAge = 0\n\tParent = null\n}";

            var person = new Person() {Name = "HELLO THERE, GENERAL KENOBI!!!"};
            
            var resultByTypes = person.PrintToString(config => config.Printing<string>().TrimToLength(5));
            var resultByName = person.PrintToString(config => config.Printing(x => x.Name).TrimToLength(5));
            
            resultByName.Should().BeEquivalentTo(expectedStr);
            resultByTypes.Should().BeEquivalentTo(expectedStr);
        }
        
        [Test]
        public void Should_StopSerializeProperty_When_PropertyInCircularRecursion()
        {
            const string expectedStr = "Recursive\n{\n\tOther = Recursive\n\t{\n\t\tOther = Recursive\n\t\t{\n\t\t\tOther = Recursive\n\t\t\t{\n\t\t\t\tOther = ...\n\t\t\t}\n\t\t}\n\t}\n}";

            var tempElement = new Recursive();
            var person = new Recursive() {Other = new Recursive() {Other = new Recursive() {Other = tempElement}}};
            tempElement.Other = person;
            
            var result = tempElement.PrintToString();
            
            result.Should().BeEquivalentTo(expectedStr);
        }
        
        [Test]
        public void Should_SerializeEnumerables_When_InputIsListEnumirables()
        {
            const string expectedStr = "List`1<Int32>\n{\n\t#0 = 0\n\t#1 = 1\n\t#2 = 2\n}";

            var result = new List<int> {0, 1, 2}.PrintToString();
            
            result.Should().BeEquivalentTo(expectedStr);
        }
        
        [Test]
        public void Should_SerializeEnumerables_When_InputIsArray()
        {
            const string expectedStr = "Int32[]<Int32>\n{\n\t#0 = 0\n\t#1 = 1\n\t#2 = 2\n}";

            var result = new[] {0, 1, 2}.PrintToString();
            
            result.Should().BeEquivalentTo(expectedStr);
        }
        
        [Test]
        public void Should_SerializeEnumerables_When_InputIsDictionaryEnumirable()
        {
            const string expectedStr = "Dictionary`2<String, Int32>\n{\n\t[a] = 1\n\t[b] = 2\n\t[c] = 3\n}";

            var result = new Dictionary<string, int> {["a"] = 1, ["b"] = 2, ["c"] = 3,}.PrintToString();
            
            result.Should().BeEquivalentTo(expectedStr);
        }
    }
}