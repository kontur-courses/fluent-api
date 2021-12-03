using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        private Person person;
        private ClassWithFields classWithFields;

        [SetUp]
        public void InitializeVariables()
        {
            person = new Person {Name = "Alex", Age = 19, Height = 0.2};
            classWithFields = new ClassWithFields() {A = 1.1, B = 2.5};
        }

        [Test]
        public void PrintToString_ContainsAllMembersOfObject_WithProperties()
        {
            var expectedPersonResult = new[]
            {
                "Person", "\tId = 00000000-0000-0000-0000-000000000000", "\tName = Alex", "\tHeight = 0,2",
                "\tAge = 19", ""
            };

            ObjectPrinter.For<Person>()
                .PrintToString(person)
                .Split(Environment.NewLine)
                .Should()
                .BeEquivalentTo(expectedPersonResult);
        }

        [Test]
        public void PrintToString_ContainsAllMembersOfObject_WithFields()
        {
            var expectedClassWithFieldsResult = new[] {"ClassWithFields", "\tB = 2,5", "\tA = 1,1", ""};

            ObjectPrinter.For<ClassWithFields>()
                .PrintToString(classWithFields)
                .Split(Environment.NewLine)
                .Should()
                .BeEquivalentTo(expectedClassWithFieldsResult);
        }

        [Test]
        public void ExceptType_NotPrintSelectedType_ForObjectWithFields()
        {
            var expectedClassWithFieldsResult = new[] {"ClassWithFields", ""};

            ObjectPrinter.For<ClassWithFields>()
                .ExceptType<double>()
                .PrintToString(classWithFields)
                .Split(Environment.NewLine)
                .Should()
                .BeEquivalentTo(expectedClassWithFieldsResult);
        }

        [Test]
        public void ExceptType_NotPrintSelectedType_ForObjectWithProperties()
        {
            var expectedPersonResult = new[]
            {
                "Person", "\tId = 00000000-0000-0000-0000-000000000000", "\tName = Alex", "\tHeight = 0,2", ""
            };

            ObjectPrinter.For<Person>()
                .ExceptType<int>()
                .PrintToString(person)
                .Split(Environment.NewLine)
                .Should()
                .BeEquivalentTo(expectedPersonResult);
        }

        [Test]
        public void ExceptMember_NotPrintsSelectedMember_ForObjectWithProperties()
        {
            var expectedPersonResult = new[]
            {
                "Person", "\tId = 00000000-0000-0000-0000-000000000000", "\tName = Alex", "\tHeight = 0,2", ""
            };

            ObjectPrinter.For<Person>()
                .ExceptMember(p => p.Age)
                .PrintToString(person)
                .Split(Environment.NewLine)
                .Should()
                .BeEquivalentTo(expectedPersonResult);
        }

        [Test]
        public void ExceptMember_NotPrintSelectedMember_ForObjectWithFields()
        {
            var expectedClassWithFieldsResult = new[] {"ClassWithFields", "\tA = 1,1", ""};

            ObjectPrinter.For<ClassWithFields>()
                .ExceptMember(c => c.B)
                .PrintToString(classWithFields)
                .Split(Environment.NewLine)
                .Should()
                .BeEquivalentTo(expectedClassWithFieldsResult);
        }

        [Test]
        public void Serialize_SetsAlternativeSerializationForSelectedType_ForObjectWithProperties()
        {
            var expectedPersonResult = new[]
            {
                "Person", "\tId = xxx", "\tName = Alex", "\tHeight = 0,2", "\tAge = 19", ""
            };

            ObjectPrinter.For<Person>()
                .Serialize<Guid>()
                .Using(g => "xxx")
                .PrintToString(person)
                .Split(Environment.NewLine)
                .Should()
                .BeEquivalentTo(expectedPersonResult);
        }

        [Test]
        public void Serialize_SetsAlternativeSerializationForSelectedType_ForObjectWithFields()
        {
            var expectedClassWithFieldsResult = new[] {"ClassWithFields", "\tB = a", "\tA = a", ""};

            ObjectPrinter.For<ClassWithFields>()
                .Serialize<double>()
                .Using(d => "a")
                .PrintToString(classWithFields)
                .Split(Environment.NewLine)
                .Should()
                .BeEquivalentTo(expectedClassWithFieldsResult);
        }

        [TestCaseSource(nameof(GetCultureTestCasesForObjectWithProperties))]
        public void Serialize_SetsCultureForNumbers_ForObjectWithProperties(string[] expectedResult,
            CultureInfo cultureInfo)
        {
            ObjectPrinter.For<Person>()
                .Serialize<double>()
                .Using(cultureInfo)
                .PrintToString(person)
                .Split(Environment.NewLine)
                .Should()
                .BeEquivalentTo(expectedResult);
        }

        public static IEnumerable<TestCaseData> GetCultureTestCasesForObjectWithProperties
        {
            get
            {
                yield return new TestCaseData(
                    new[]
                    {
                        "Person", "\tId = 00000000-0000-0000-0000-000000000000", "\tName = Alex", "\tHeight = 0,2",
                        "\tAge = 19", ""
                    }, CultureInfo.CurrentCulture);
                yield return new TestCaseData(
                    new[]
                    {
                        "Person", "\tId = 00000000-0000-0000-0000-000000000000", "\tName = Alex", "\tHeight = 0.2",
                        "\tAge = 19", ""
                    }, CultureInfo.InvariantCulture);
            }
        }

        [TestCaseSource(nameof(GetCultureTestCasesForObjectWithFields))]
        public void Serialize_SetsCultureForNumbers_ForObjectWithFields(string[] expectedResult,
            CultureInfo cultureInfo)
        {
            ObjectPrinter.For<ClassWithFields>()
                .Serialize<double>()
                .Using(cultureInfo)
                .PrintToString(classWithFields)
                .Split(Environment.NewLine)
                .Should()
                .BeEquivalentTo(expectedResult);
        }

        public static IEnumerable<TestCaseData> GetCultureTestCasesForObjectWithFields
        {
            get
            {
                yield return new TestCaseData(new[] {"ClassWithFields", "\tB = 2,5", "\tA = 1,1", ""},
                    CultureInfo.CurrentCulture);
                yield return new TestCaseData(new[] {"ClassWithFields", "\tB = 2.5", "\tA = 1.1", ""},
                    CultureInfo.InvariantCulture);
            }
        }

        [Test]
        public void Serialize_SetsSerializationForSelectedProperty_ForObjectWithProperties()
        {
            var expectedPersonResult = "Person" + Environment.NewLine + "\tId = lol" + Environment.NewLine +
                                       "\tName = Alex" + Environment.NewLine + "\tHeight = 0,2" + Environment.NewLine +
                                       "\tAge = 19" + Environment.NewLine;

            ObjectPrinter.For<Person>()
                .Serialize(p => person.Id)
                .Using(i => "lol")
                .PrintToString(person)
                .Should()
                .BeEquivalentTo(expectedPersonResult);
        }

        [Test]
        public void Serialize_SetsSerializationForSelectedProperty_ForObjectWithFields()
        {
            var expectedClassWithFieldsResult = new[] {"ClassWithFields", "\tB = 2,5", "\tA = a", ""};

            ObjectPrinter.For<ClassWithFields>()
                .Serialize(c => c.A)
                .Using(i => "a")
                .PrintToString(classWithFields)
                .Split(Environment.NewLine)
                .Should()
                .BeEquivalentTo(expectedClassWithFieldsResult);
        }

        [Test]
        public void SettingCulture_HasLowerPriority_ThanAlternativeTypeSerialization()
        {
            var expectedPersonResult = new[]
            {
                "Person", "\tId = 00000000-0000-0000-0000-000000000000", "\tName = Alex", "\tHeight = a",
                "\tAge = 19", ""
            };

            ObjectPrinter.For<Person>()
                .Serialize<double>()
                .Using(CultureInfo.InvariantCulture)
                .Serialize<double>()
                .Using(x => "a")
                .PrintToString(person)
                .Split(Environment.NewLine)
                .Should()
                .BeEquivalentTo(expectedPersonResult);
        }

        [Test]
        public void AlternativeTypeSerialization_HasLowerPriority_ThanAlternativePropertySerialization()
        {
            var expectedPersonResult = new[]
            {
                "Person", "\tId = 00000000-0000-0000-0000-000000000000", "\tName = Alex", "\tHeight = b",
                "\tAge = 19", ""
            };

            ObjectPrinter.For<Person>()
                .Serialize<double>()
                .Using(x => "a")
                .Serialize(p => p.Height)
                .Using(x => "b")
                .PrintToString(person)
                .Split(Environment.NewLine)
                .Should()
                .BeEquivalentTo(expectedPersonResult);
        }

        [Test]
        public void AlternativePropertySerialization_HasLowerPriority_ThanExcluding()
        {
            var expectedPersonResult = new[]
            {
                "Person", "\tId = 00000000-0000-0000-0000-000000000000", "\tName = Alex", "\tAge = 19", ""
            };

            ObjectPrinter.For<Person>()
                .Serialize(p => p.Height)
                .Using(x => "b")
                .ExceptType<double>()
                .PrintToString(person)
                .Split(Environment.NewLine)
                .Should()
                .BeEquivalentTo(expectedPersonResult);
        }

        [Test]
        public void ResultNotDependsOnOrderOfCommands()
        {
            var expectedPersonResult = new[]
            {
                "Person", "\tId = 00000000-0000-0000-0000-000000000000", "\tName = a", "\tHeight = b", ""
            };
            var firstOrderResult = ObjectPrinter.For<Person>()
                .Serialize(p => p.Height)
                .Using(x => "b")
                .ExceptType<int>()
                .Serialize<string>()
                .Using(s => "a")
                .PrintToString(person)
                .Split(Environment.NewLine);
            var secondOrderResult = ObjectPrinter.For<Person>()
                .ExceptType<int>()
                .Serialize<string>()
                .Using(s => "a")
                .Serialize(p => p.Height)
                .Using(x => "b")
                .PrintToString(person)
                .Split(Environment.NewLine);

            firstOrderResult.Should().BeEquivalentTo(secondOrderResult);
            secondOrderResult.Should().BeEquivalentTo(expectedPersonResult);
        }
        
        [Test]
        public void PrintToString_NotThrows_WhenObjectHasCyclicLinks()
        {
            var cyclicalLinkObject = new ClassWithCyclicalLink();
            Action act = () => ObjectPrinter.For<ClassWithCyclicalLink>().PrintToString(cyclicalLinkObject);

            act.Should().NotThrow<StackOverflowException>();
        }
        
        [Test]
        public void PrintToString_PrintWordsWithCyclicLinksCorrectly()
        {
            var cyclicalLinkObject = new ClassWithCyclicalLink();
            var expectedResult = "ClassWithCyclicalLink" + Environment.NewLine + "\tCyclicalLinkObject = object with cyclic link" + Environment.NewLine;
            ObjectPrinter.For<ClassWithCyclicalLink>().PrintToString(cyclicalLinkObject).Should()
                .BeEquivalentTo(expectedResult);
        }
        
        [Test]
        public void Serialize_SetBoundsOfMemberSerialization()
        {
            var expectedPersonResult = new[]
            {
                "Person", "\tId = 00000000-0000-0000-0000-000000000000", "\tName = Al", "\tHeight = 0,2",
                "\tAge = 19", ""
            };
            
            
            ObjectPrinter.For<Person>().Serialize(p => p.Name).WithBounds(0, 1).PrintToString(person).Split(Environment.NewLine).Should()
                .BeEquivalentTo(expectedPersonResult);
        }
        
        [TestCase(-1, 2, TestName = "start is negative")]
        [TestCase(1, 9999, TestName = "end is more than length")]
        [TestCase(2, 1, TestName = "end is less than start")]
        public void Serialize_Throw_WhenBoundsOfSerializationAreInvalid(int start, int end)
        {
            Action act = () =>
                ObjectPrinter.For<Person>().Serialize(p => p.Name).WithBounds(start, end).PrintToString(person);
            act.Should().Throw<ArgumentException>();
        }
    }
}