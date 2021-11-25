using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Contexts;

namespace ObjectPrintingTests
{
    public class ObjectPrinterTests
    {
        private Person person;
        private NestedPerson nestedPerson;

        private ConfigPrintingContext<Person> personPrinter;

        private ObjectDescription personDesc;
        private string id;
        private string name;
        private string height;
        private string age;

        [SetUp]
        public void SetUp()
        {
            person = new Person {Name = "Alex", Age = 19, Height = 2.1};
            nestedPerson = new NestedPerson {Name = "Mike", Age = 99, Height = 2.2, Child = person};

            personPrinter = ObjectPrinter.For<Person>();

            personDesc = new ObjectDescription("Person");
            id = "Id = Guid";
            name = "Name = Alex";
            height = "Height = 2,1";
            age = "Age = 19";
        }

        [Test]
        public void AcceptanceTest()
        {
            ObjectPrinter.For<Person>()
                //1 Исключение из сериализации свойства/поля определенного типа
                .Excluding<Guid>()
                //2 Альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString())
                //3 Для всех типов, имеющих культуру, есть возможность ее указать
                .FormatFor<double>(CultureInfo.InvariantCulture)
                //4 Настройка сериализации конкретного свойства/поля
                .Printing(p => p.Height).Using(propInfo => propInfo.Name)
                //5 Возможность обрезания строк
                .MaxStringLength(10)
                //6 Исключение из сериализации конкретного свойства/поля
                .Excluding(p => p.Age)
                .PrintToString(person);
        }

        [Test]
        public void ObjectPrinter_IsImmutable()
        {
            var expectedWithType = personDesc.WithFields(id, name, height, "Age = CustomInt").ToString();
            var expectedWithProp = personDesc.WithFields(id, name, height, "Age").ToString();

            var withCustomType = personPrinter.Printing<int>().Using(i => "CustomInt");
            var withCustomProp = personPrinter.Printing(p => p.Age).Using(prop => prop.Name);

            withCustomType.PrintToString(person).Should().Be(expectedWithType);
            withCustomProp.PrintToString(person).Should().Be(expectedWithProp);
        }

        [Test]
        public void PrintToString_WithoutConfigOneNestingLevel()
        {
            var expected = personDesc
                .WithFields(id, name, height, age)
                .ToString();


            personPrinter.PrintToString(person)
                .Should().Be(expected);
        }

        [Test]
        public void PrintToString_WithoutConfigTwoNestingLevels()
        {
            var expected = new ObjectDescription("NestedPerson")
                .WithFields(new ObjectDescription("Child = Person")
                    .WithFields(id, name, height, age))
                .WithFields(
                    "Id = Guid",
                    "Name = Mike",
                    "Height = 2,2",
                    "Age = 99")
                .ToString();

            var printer = ObjectPrinter.For<NestedPerson>();

            printer.PrintToString(nestedPerson)
                .Should().Be(expected);
        }

        [Test]
        public void PrintToString_Null()
        {
            ObjectPrinter.For<Person>()
                .PrintToString(null)
                .Should().Be("null" + Environment.NewLine);
        }

        [Test]
        public void Excluding_Types()
        {
            var expected = personDesc
                .WithFields(id, age)
                .ToString();

            var printer = personPrinter
                .Excluding<double>()
                .Excluding<string>();

            printer.PrintToString(person)
                .Should().Be(expected);
        }

        [Test]
        public void Excluding_Properties()
        {
            var expected = personDesc
                .WithFields(id, name)
                .ToString();

            var printer = personPrinter
                .Excluding(p => p.Age)
                .Excluding(p => p.Height);

            printer.PrintToString(person)
                .Should().Be(expected);
        }

        [Test]
        public void Printing_UsingCustomTypeSerialization()
        {
            var expected = personDesc
                .WithFields(id, name, height, "Age = My int 19")
                .ToString();

            var printer = personPrinter
                .Printing<int>().Using(i => $"My int {i}");

            printer.PrintToString(person)
                .Should().Be(expected);
        }

        [Test]
        public void Printing_FormatFor()
        {
            var expected = personDesc
                .WithFields(id, name, "Height = 2.1", age)
                .ToString();

            var printer = personPrinter
                .FormatFor<double>(CultureInfo.InvariantCulture);

            printer.PrintToString(person)
                .Should().Be(expected);
        }

        [Test]
        public void Printing_UsingCustomPropertySerialization()
        {
            var expected = personDesc
                .WithFields(id, "Name!", height, age)
                .ToString();

            var printer = personPrinter
                .Printing(p => p.Name).Using(propertyInfo => propertyInfo.Name + "!");

            printer.PrintToString(person)
                .Should().Be(expected);
        }

        [TestCaseSource(nameof(TrimStringsCases))]
        public string TrimStrings(string input, int length)
        {
            var printer = ObjectPrinter.For<string>()
                .MaxStringLength(length);

            return printer.PrintToString(input);
        }

        private static IEnumerable<TestCaseData> TrimStringsCases()
        {
            const char ellipsis = '\u2026';
            yield return new TestCaseData(new string('a', 10), 5)
            {
                ExpectedResult = new string('a', 5) + ellipsis + Environment.NewLine,
                TestName = "Should add ellipsis if string long"
            };

            yield return new TestCaseData(new string('a', 10), 20)
            {
                ExpectedResult = new string('a', 10) + Environment.NewLine,
                TestName = "Don't add ellipsis if string short"
            };
        }
    }
}