using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    class ObjectPrinting_Should
    {
        private PrintingConfig<Person> personPrinter;
        private Person person;

        private PrintingConfig<MyCustomObject> myCustomObjectPrinter;
        private MyCustomObject myCustomObject;

        private MyObjectWithEnumerables myObjectWithEnumerables;
        private PrintingConfig<MyObjectWithEnumerables> myObjectWithEnumerablesPrinter;

        [SetUp]
        public void SetUp()
        {
            personPrinter = ObjectPrinter.For<Person>();
            person = new Person
            {
                Id = Guid.Empty,
                Name = "John Smith",
                Age = 69,
                Height = 13.37
            };

            myCustomObjectPrinter = ObjectPrinter.For<MyCustomObject>();
            myCustomObject = new MyCustomObject
            {
                JustAField = 1337,
                StringProperty = "string",
                AnotherStringProperty = "another string",
                Person = person
            };

            myObjectWithEnumerablesPrinter = ObjectPrinter.For<MyObjectWithEnumerables>();
            myObjectWithEnumerables = new MyObjectWithEnumerables
            {
                Array = new[] {1, 2, 3},
                ListOfComplexObjects = new List<Person>
                {
                    person,
                    person
                },
                Dictionary = new Dictionary<string, string>
                {
                    { "Somebody once told me", "the world is gonna roll me" },
                    { "I ain't the sharpest", "tool in the shed" }
                }
            };
        }

        [Test]
        public void ObjectPrinter_ShouldExcludeTypeFromSerialization()
        {
            var expected = string.Join(Environment.NewLine, "Person", "\tId = 00000000-0000-0000-0000-000000000000", 
                               "\tName = John Smith", "\tHeight = 13,37")
                           + Environment.NewLine;

            personPrinter.Excluding<int>();

            personPrinter.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_ShouldSerializeTypesAlternatively()
        {
            var expected = string.Join(Environment.NewLine, "Person", "\tId = 00000000-0000-0000-0000-000000000000",
                               "\tName = John Smith", "\tHeight = 13,37", "\tAge = 69 (это инт)")
                           + Environment.NewLine;

            personPrinter.Printing<int>().Using(e => e + " (это инт)");

            personPrinter.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_ShouldSetCulture()
        {
            var expected = string.Join(Environment.NewLine, "Person", "\tId = 00000000-0000-0000-0000-000000000000",
                               "\tName = John Smith", "\tHeight = 13.37", "\tAge = 69")
                           + Environment.NewLine;

            personPrinter.Printing<double>().Using(CultureInfo.InvariantCulture);

            personPrinter.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_ShouldThrowArgumentException_WhenSettingCultureOnNotNumericTypes()
        {
            Action action = () => personPrinter.Printing<string>().Using(CultureInfo.InvariantCulture);

            action.Should().Throw<ArgumentException>()
                .WithMessage("Использованный тип не является допустимым.");

        }


        [Test]
        public void ObjectPrinter_ShouldSerializePropertiesAlternatively()
        {
            var expected = string.Join(Environment.NewLine, "MyCustomObject", "\tJustAField = 1337",
                               "\tStringProperty = string (это строковое свойство)",
                               "\tAnotherStringProperty = another string",
                               "\tPerson = Person",
                               "\t\tId = 00000000-0000-0000-0000-000000000000",
                               "\t\tName = John Smith",
                               "\t\tHeight = 13,37",
                               "\t\tAge = 69")
                           + Environment.NewLine;

            myCustomObjectPrinter.Printing(e => e.StringProperty).Using(e => e + " (это строковое свойство)");

            myCustomObjectPrinter.PrintToString(myCustomObject).Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_ShouldTrimStringProperties()
        {
            var expected = string.Join(Environment.NewLine, "Person", "\tId = 00000000-0000-0000-0000-000000000000", 
                               "\tName = John", "\tHeight = 13,37", "\tAge = 69")
                           + Environment.NewLine;

            personPrinter.Printing(e => e.Name).TrimmedToLength(4);

            personPrinter.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_ShouldExcludePropertyFromSerialization()
        {
            var expected = string.Join(Environment.NewLine, "Person", "\tName = John Smith", "\tHeight = 13,37", "\tAge = 69")
                           + Environment.NewLine;

            personPrinter.Excluding(e => e.Id);

            personPrinter.PrintToString(person).Should().Be(expected);
        }


        [Test]
        public void ObjectPrinter_ShouldOverrideAlternativeTypeSerialization_IfPropertySerializationSpecified()
        {
            var expected = string.Join(Environment.NewLine, "MyCustomObject", "\tJustAField = 1337",
                               "\tStringProperty = string (это StringProperty (форматирование для типа не применилось))",
                               "\tAnotherStringProperty = another string (это строка)",
                               "\tPerson = Person",
                               "\t\tId = 00000000-0000-0000-0000-000000000000",
                               "\t\tName = John Smith (это строка)",
                               "\t\tHeight = 13,37",
                               "\t\tAge = 69")
                           + Environment.NewLine;
            
            myCustomObjectPrinter
                .Printing(e => e.StringProperty).Using(e => e + " (это StringProperty (форматирование для типа не применилось))")
                .Printing<string>().Using(e => e + " (это строка)");

            myCustomObjectPrinter.PrintToString(myCustomObject).Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_ShouldThrowArgumentException_OnInvalidExpressionInPrinting()
        {
            Action action = () => personPrinter.Printing(x => new[] { 1, 2, 3 });

            action.Should().Throw<ArgumentException>()
                .WithMessage("Использованное выражение не является допустимым");
        }

        [Test]
        public void ObjectPrinter_ShouldThrowArgumentException_OnInvalidExpressionInExcluding()
        {
            Action action = () => personPrinter.Excluding(x => x.Age + 1);

            action.Should().Throw<ArgumentException>()
                .WithMessage("Использованное выражение не является допустимым");
        }

        [Test]
        public void ObjectPrinter_ShouldSerializeFields()
        {
            var expected = string.Join(Environment.NewLine, "MyCustomObject", "\tJustAField = 1337",
                               "\tStringProperty = string",
                               "\tAnotherStringProperty = another string",
                               "\tPerson = Person",
                               "\t\tId = 00000000-0000-0000-0000-000000000000",
                               "\t\tName = John Smith",
                               "\t\tHeight = 13,37",
                               "\t\tAge = 69")
                           + Environment.NewLine;

            myCustomObjectPrinter.PrintToString(myCustomObject).Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_ShouldSerializeObjectsWithEnumerations()
        {
            var expected = string.Join(Environment.NewLine, "MyObjectWithEnumerables",
                               "\tArray = Int32[]",
                                    "\t\t[0] = 1",
                                    "\t\t[1] = 2",
                                    "\t\t[2] = 3",
                               "\tListOfComplexObjects = List`1",
                                    "\t\t[0] = Person",
                                        "\t\t\tId = 00000000-0000-0000-0000-000000000000",
                                        "\t\t\tName = John Smith",
                                        "\t\t\tHeight = 13,37",
                                        "\t\t\tAge = 69",
                                    "\t\t[1] = Person",
                                        "\t\t\tId = 00000000-0000-0000-0000-000000000000",
                                        "\t\t\tName = John Smith",
                                        "\t\t\tHeight = 13,37",
                                        "\t\t\tAge = 69",
                               "\tDictionary = Dictionary`2",
                                    "\t\t[0] = [Somebody once told me, the world is gonna roll me]",
                                    "\t\t[1] = [I ain't the sharpest, tool in the shed]"
                               )
                           + Environment.NewLine;

            myObjectWithEnumerablesPrinter.PrintToString(myObjectWithEnumerables).Should().Be(expected);
        }

    }
}
