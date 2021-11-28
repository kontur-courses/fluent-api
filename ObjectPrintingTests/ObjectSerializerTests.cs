using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Contexts;
using ObjectPrintingTests.Helpers;
using ObjectPrintingTests.Persons;

namespace ObjectPrintingTests
{
    public class ObjectSerializerTests
    {
        private Person person;

        private ConfigPrintingContext<Person> personPrinter;

        private ObjectDescription personDesc;
        private string id;
        private string name;
        private string height;
        private string age;

        [SetUp]
        public void SetUp()
        {
            person = new Person();
            personPrinter = ObjectPrinter.For<Person>();
            personDesc = new ObjectDescription(nameof(Person));
            (id, name, height, age) = PersonDescription.GetPersonFields(person);
        }

        [Test]
        public void AcceptanceTest()
        {
            ObjectPrinter.For<Person>()
                //1 Исключение из сериализации свойства/поля определенного типа
                .Excluding<Guid>()
                //2 Альтернативный способ сериализации для определенного типа
                .Printing<string>().Using(s => s + "!!!")
                //3 Для всех типов, имеющих культуру, есть возможность ее указать
                .FormatFor<int>("X", CultureInfo.InvariantCulture)
                //4 Настройка сериализации конкретного свойства/поля
                .Printing(p => p.Height).Using(memberInfo => memberInfo.Module.Name)
                //5 Возможность обрезания строк
                .MaxStringLength(10)
                //6 Исключение из сериализации конкретного свойства/поля
                .Excluding(p => p.Name)
                .PrintToString(person);
        }

        [Test]
        public void PrintToString_Overview()
        {
            var collectionPerson = new CollectionPerson();
            var nestedPerson = new NestedPerson {Child = person};

            collectionPerson.ChildrenArray = new[] {nestedPerson, person};
            collectionPerson.ChildrenList = new List<Person>();
            collectionPerson.ChildrenDictionary = new Dictionary<int, Person> {[1] = new()};

            var dictionaryPerson = new DictionaryPerson
            {
                Persons = new Dictionary<NestedPerson, CollectionPerson>
                {
                    [nestedPerson] = collectionPerson
                }
            };

            var actual = ObjectPrinter.For<DictionaryPerson>()
                .Excluding<int>()
                .Printing<double>().Using(value => value + "!!!")
                .FormatFor<Guid>("X", CultureInfo.InvariantCulture)
                .PrintToString(dictionaryPerson);

            Console.WriteLine(actual);
        }

        [Test]
        public void ObjectPrinter_IsImmutable()
        {
            var expectedWithType = personDesc.WithFields(id, name, height, "Age = CustomInt").ToString();
            var expectedWithProp = personDesc.WithFields(id, name, height, "Age").ToString();

            var withCustomType = personPrinter.Printing<int>().Using(_ => "CustomInt");
            var withCustomProp = personPrinter.Printing(p => p.Age).Using(prop => prop.Name);

            withCustomType.PrintToString(person).Should().Be(expectedWithType);
            withCustomProp.PrintToString(person).Should().Be(expectedWithProp);
        }

        [Test]
        public void PrintToString_WithoutConfigOneNestingLevel()
        {
            var expected = PersonDescription.GetDefaultDescription(person).ToString();

            personPrinter.PrintToString(person)
                .Should().Be(expected);
        }

        [Test]
        public void PrintToString_WithoutConfigTwoNestingLevels()
        {
            var expected = new ObjectDescription("NestedPerson")
                .WithFields(new ObjectDescription("Child = Person")
                    .WithFields(id, name, height, age)
                    .WithOffset("Child = ".Length))
                .WithFields(
                    id,
                    "Name = Mike",
                    "Height = 2,2",
                    "Age = 99")
                .ToString();

            var nestedPerson = new NestedPerson {Name = "Mike", Age = 99, Height = 2.2, Child = person};
            var printer = ObjectPrinter.For<NestedPerson>();
            var actual = printer.PrintToString(nestedPerson);

            actual.Should().Be(expected);
            Console.WriteLine(actual);
        }

        [Test]
        public void PrintToString_Null() =>
            ObjectPrinter.For<Person>()
                .PrintToString(null)
                .Should().Be("null");

        [Test]
        public void PrintToString_AlsoWorkWithFields()
        {
            var expected = new ObjectDescription(nameof(FieldPerson))
                .WithFields("Height", id, name)
                .ToString();

            var filedPerson = new FieldPerson(Guid.Empty, "Alex", 2.1, 19);
            var printer = ObjectPrinter.For<FieldPerson>()
                .Printing(p => p.Height).Using(propInfo => propInfo.Name)
                .Excluding(p => p.Age);

            printer.PrintToString(filedPerson)
                .Should().Be(expected);
        }

        [Test]
        public void PrintToString_IgnorePrivateFields()
        {
            var expected = new ObjectDescription(nameof(PrivatePerson))
                .WithFields(id)
                .ToString();

            var printer = ObjectPrinter.For<PrivatePerson>();

            printer.PrintToString(new PrivatePerson())
                .Should().Be(expected);
        }

        [Test]
        public void PrintToString_WithCycleReference_DontThrowException()
        {
            var cyclePerson = new NestedPerson {Age = 99, Height = 9.9, Name = "Mike", Id = Guid.Empty};
            cyclePerson.Child = cyclePerson;

            ObjectPrinter.For<NestedPerson>()
                .PrintToString(cyclePerson);
        }

        [Test]
        public void PrintToString_WorkWithTime()
        {
            var birthDay = new DateTime(1000, 10, 20);
            var weekWorkTime = new TimeSpan(987);
            var expected = new ObjectDescription(nameof(TimePerson))
                .WithFields(
                    $"{nameof(TimePerson.BirthDay)} = {birthDay.ToString()}",
                    $"{nameof(TimePerson.WeekWorkTime)} = {weekWorkTime.ToString()}")
                .ToString();

            var timePerson = new TimePerson(birthDay, weekWorkTime);
            var printer = ObjectPrinter.For<TimePerson>();

            printer.PrintToString(timePerson)
                .Should().Be(expected);
        }

        [Test]
        public void PrintToString_Excluding_Types()
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
        public void PrintToString_Excluding_Properties()
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
        public void Excluding_ThrowsException_IfFunctionMemberSelected() =>
            Assert.That(() =>
            {
                ObjectPrinter.For<FuncPerson>()
                    .Excluding(p => p.GetAge());
            }, Throws.InstanceOf<ArgumentException>());

        [Test]
        public void PrintToString_Printing_UsingCustomTypeSerialization()
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
        public void PrintToString_Printing_SetFormatForFormattable()
        {
            var expected = personDesc
                .WithFields(id, name, height, $"{nameof(person.Age)} = 13")
                .ToString();

            var printer = personPrinter
                .FormatFor<int>("X", CultureInfo.InvariantCulture);

            printer.PrintToString(person)
                .Should().Be(expected);
        }

        [Test]
        public void PrintToString_Printing_UsingCustomPropertySerialization()
        {
            var expected = personDesc
                .WithFields(id, "Name!", height, age)
                .ToString();

            var printer = personPrinter
                .Printing(p => p.Name).Using(memberInfo => memberInfo.Name + "!");

            printer.PrintToString(person)
                .Should().Be(expected);
        }

        [TestCaseSource(nameof(TrimStringsCases))]
        public string PrintToString_TrimStrings(string input, int length)
        {
            var printer = ObjectPrinter.For<string>()
                .MaxStringLength(length);

            return printer.PrintToString(input);
        }

        [TestCaseSource(nameof(PrintToStringWorkWithCollectionsCases))]
        public void PrintToString_WorkWithCollections(CollectionPerson collectionPerson, string expected)
        {
            var printer = ObjectPrinter.For<CollectionPerson>();

            var actual = printer.PrintToString(collectionPerson);

            actual.Should().Contain(expected);
            Console.WriteLine(actual);
        }

        private static IEnumerable<TestCaseData> TrimStringsCases()
        {
            const char ellipsis = '\u2026';
            yield return new TestCaseData(new string('a', 10), 5)
            {
                ExpectedResult = new string('a', 5) + ellipsis,
                TestName = "Should add ellipsis if string long"
            };

            yield return new TestCaseData(new string('a', 10), 20)
            {
                ExpectedResult = new string('a', 10),
                TestName = "Don't add ellipsis if string short"
            };
        }

        private static IEnumerable<TestCaseData> PrintToStringWorkWithCollectionsCases()
        {
            var personWithArray = new CollectionPerson {ChildrenArray = new Person[] {new(), new()}};
            yield return new TestCaseData(personWithArray,
                    GetExpectedForPrintToStringWorkWithCollections("Array",
                        nameof(CollectionPerson.ChildrenArray), 2))
                {TestName = "Array"};

            var personWithList = new CollectionPerson {ChildrenList = new List<Person> {new(), new()}};
            yield return new TestCaseData(personWithList,
                    GetExpectedForPrintToStringWorkWithCollections("List",
                        nameof(CollectionPerson.ChildrenList), 2))
                {TestName = "List"};


            var personWithDictionary = new CollectionPerson
                {ChildrenDictionary = new Dictionary<int, Person> {[0] = new(), [1] = new()}};

            yield return new TestCaseData(personWithDictionary,
                    GetExpectedForPrintToStringWorkWithCollections("Dictionary",
                        nameof(CollectionPerson.ChildrenDictionary), 2))
                {TestName = "Dictionary"};
        }

        private static string GetExpectedForPrintToStringWorkWithCollections(string header, string collectionName,
            int count)
        {
            var (id, name, height, age) = PersonDescription.GetPersonFields(new Person());
            var fields = new List<ObjectDescription>();
            for (var i = 0; i < count; i++)
            {
                fields.Add(
                    new ObjectDescription(
                            new string(' ', $"{collectionName} = ".Length) + $"[{i}]: {nameof(Person)}")
                        .WithFields(id, name, height, age)
                        .WithOffset($"{collectionName} = [{i}]: ".Length));
            }

            return new ObjectDescription($"{header}[{count}]")
                .WithFields(fields.ToArray())
                .ToString();
        }
    }
}