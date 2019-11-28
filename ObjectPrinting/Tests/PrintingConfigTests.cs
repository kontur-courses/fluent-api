using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class PrintingConfigTests
    {
        private static readonly Person Person = new Person {Name = "Alex", Age = 19, Height = 170, Phone = "123456789"};

        [TestCase(typeof(string), Description = "Exclude string")]
        [TestCase(typeof(int), Description = "Exclude int")]
        [TestCase(typeof(Guid), Description = "Exclude Guid")]
        public void PrintToString_DoesNotPrintProperties_WhenTheirTypeIsExcluded(Type type)
        {
            var config = new PrintingConfig<Person>();

            var result = (typeof(PrintingConfig<Person>)
                    .GetMethod("Excluding", new Type[] { })
                    ?.MakeGenericMethod(type)
                    .Invoke(config, null) as PrintingConfig<Person>)
                ?.PrintToString(Person);

            foreach (var property in Person.GetElements().Where(p => p.Type == type))
                result.Should()
                    .NotContain(property.MemberInfo.Name).And
                    .NotContain(property.GetValue(Person).ToString());
        }

        private static IEnumerable<Func<string, string>> GetStringPrintingTestCases()
        {
            yield return s => string.Join(" ", s.ToCharArray());
        }

        private static IEnumerable<Func<Guid, string>> GetGuidPrintingTestCases()
        {
            yield return id => id.ToString();
        }

        [TestCaseSource(nameof(GetStringPrintingTestCases))]
        [TestCaseSource(nameof(GetGuidPrintingTestCases))]
        public void PrintToString_UsesAlternativeSerialization_ForIndicatedType<T>(Func<T, string> print)
        {
            var config = new PrintingConfig<Person>();

            var result = config.Printing<T>().Using(print).PrintToString(Person);

            foreach (var property in Person.GetElements().Where(p => p.Type == typeof(T)))
                result.Should()
                    .Contain(property.MemberInfo.Name).And
                    .Contain(print((T) property.GetValue(Person)));
        }

        [TestCase("Mike", 10, Description = "Length is less than maxLen")]
        [TestCase("Cameron", 5, Description = "Length is greater than maxLen")]
        public void PrintToString_TruncatesStrings_LongerThanMaxLen(string name, int maxLen)
        {
            var person = new Person {Name = name};
            var config = new PrintingConfig<Person>();

            var result = config.Printing<string>().TrimmedToLength(maxLen).PrintToString(person);

            if (maxLen >= name.Length)
                result.Should().Contain(name);
            else
                result.Should().NotContain(name).And.Contain(name.Substring(0, maxLen));
        }

        [Test]
        public void PrintToString_DoesNotPrintProperties_WhenTheyAreExcluded()
        {
            var config = new PrintingConfig<Person>();

            var result = config.Excluding(p => p.Name).PrintToString(Person);

            result.Should()
                .NotContain("Name").And.NotContain(Person.Name).And
                .Contain("Phone").And.Contain(Person.Phone);
        }

        [Test]
        public void PrintToString_PrintsNumbers_UsingTheSpecifiedCultureInfo()
        {
            var culture = CultureInfo.CurrentCulture;
            var config = new PrintingConfig<Person>();

            var result = config.Printing<int>().Using(culture).PrintToString(Person);

            foreach (var property in Person.GetElements().Where(p => p.Type == typeof(int)))
                result.Should()
                    .Contain(property.MemberInfo.Name).And
                    .Contain(((int) property.GetValue(Person)).ToString(culture));
        }

        [Test]
        public void PrintToString_PrintsPublicElements_WithoutConfiguration()
        {
            var config = new PrintingConfig<Person>();

            var result = config.PrintToString(Person);

            result.Should()
                .Contain("Name").And.Contain(Person.Name).And
                .Contain("Age").And.Contain(Person.Age.ToString()).And
                .Contain("Phone").And.Contain(Person.Phone);
        }

        [Test]
        public void PrintToString_UsesPrintingFunction_ForTheSpecifiedProperty()
        {
            var config = new PrintingConfig<Person>();

            string Print(string name)
            {
                return $"(c) {name}";
            }

            var result = config
                .Printing(p => p.Name)
                .Using(Print)
                .PrintToString(Person);

            result.Should()
                .Contain("Name").And
                .Contain(Print(Person.Name)).And
                .Contain("Phone").And
                .Contain(Person.Phone).And
                .NotContain(Print(Person.Phone));
        }

        [Test]
        public void PrintToString_PrintsNull_IfPropertyIsNull()
        {
            var person = new Person {Name = null, Phone = ""};
            var config = new PrintingConfig<Person>();

            var result = config
                .Printing(p => p.Name)
                .TrimmedToLength(5)
                .Excluding(p => p.Id)
                .Excluding(p => p.BestFriend)
                .PrintToString(person);

            result.Should()
                .Contain("Name").And
                .Contain("null");
        }

        [Test]
        [Timeout(1000)]
        public void PrintToString_HandlesCircularReferences_ObjectRefersToItself()
        {
            var person = new Person();
            person.BestFriend = person;
            var config = new PrintingConfig<Person>();

            var result = config.PrintToString(person);

            foreach (var property in Person.GetElements().Where(p => p.Type != typeof(Person)))
                result.Should()
                    .Contain(property.MemberInfo.Name).And
                    .Contain(property.GetValue(person).PrintToString());
        }

        [Test]
        [Timeout(1000)]
        public void PrintToString_HandlesCircularReferences_TwoLinkedObjects()
        {
            var person = new Person();
            person.BestFriend = new Person {BestFriend = person};
            var config = new PrintingConfig<Person>();

            var result = config.PrintToString(person);

            foreach (var property in Person.GetElements().Where(p => p.Type != typeof(Person)))
                result.Should()
                    .Contain(property.MemberInfo.Name).And
                    .Contain(property.GetValue(person).PrintToString());
        }

        private static IList<string>[] _indexedCollectionTestCases =
        {
            new[] {"one", "two", "three", "four", "five"},
            new List<string> {"six", "seven", "eight", "nine", "ten"}
        };

        [TestCaseSource(nameof(_indexedCollectionTestCases))]
        public void PrintToString_PrintsCollection_WhenCollectionIsIndexed(IList<string> collection)
        {
            var config = new PrintingConfig<IList<string>>();

            var result = config.PrintToString(collection);

            for (var i = 0; i < collection.Count; i++)
                result.Should()
                    .Contain(i.ToString()).And
                    .Contain(collection[i]);
        }
    }
}