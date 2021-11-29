using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Tests.TestClasses;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private readonly PersonWithParent personWithParent = new()
        {
            Name = "Alex",
            Age = 19,
            Height = 70.5,
            Id = new Guid(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11),
            Surname = "Seshovich",
            Parent = new PersonWithParent
            {
                Name = "Kostya",
                Age = 28,
                Height = 80,
                Id = new Guid(11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1)
            }
        };

        private readonly Person person = new()
        {
            Name = "Alex",
            Age = 19,
            Height = 70.5,
            Id = new Guid(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11),
            Surname = "Alexovich"
        };

        private readonly ObjectWithEnumerable objectWithList = new()
        {
            Collection = new List<int> { 1, 2, 3 }
        };

        private readonly ObjectWithEnumerable objectWithHashSet = new()
        {
            Collection = new HashSet<int> { 1, 2, 3 }
        };

        private readonly ObjectWithDictionary objectWithDictionary = new()
        {
            Dictionary = new Dictionary<string, int>
            {
                { "One", 1 },
                { "Two", 2 },
                { "Three", 3 }
            }
        };

        private static PersonWithParent[] GetPersonsWithParentWithCycleLink()
        {
            var persons = new PersonWithParent[2];

            var personWithCycleLink = new PersonWithParent
            {
                Name = "Alex",
                Age = 19,
                Height = 70.5,
                Id = new Guid(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11),
                Surname = "Seshovich"
            };

            var personWithCycleLink2 = new PersonWithParent
            {
                Name = "Kostya",
                Age = 28,
                Height = 80,
                Id = new Guid(11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1),
                Surname = "Seshovich",
                Parent = personWithCycleLink
            };

            personWithCycleLink.Parent = personWithCycleLink2;

            persons[0] = personWithCycleLink;
            persons[1] = personWithCycleLink2;

            return persons;
        }

        [Test]
        public void Demo()
        {
            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString("X"))
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Age).Using(_ => "Age")
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(10)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Height)
                //7. Уставноить максимальный уровень рекурсии
                .SetMaxNestingLevel(10);

            var s1 = printer.PrintToString(person);

            //8. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            var s2 = person.PrintToString();

            //9. ...с конфигурированием
            var s3 = person.PrintToString(s => s.Excluding(p => p.Age));

            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }

        [Test]
        public void ExcludeType_ShouldExcludeSelectedType()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<int>();

            var actual = printer.PrintToString(person);

            actual.Should().BeEquivalentTo("Person" +
                                           "\r\n\tId = 00000001-0002-0003-0405-060708090a0b" +
                                           "\r\n\tName = Alex" +
                                           "\r\n\tHeight = 70,5" +
                                           "\r\n\tSurname = Alexovich" +
                                           "\r\n");
        }

        [Test]
        public void Using_ShouldUseAlternativeWayToDeserialize()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(_ => "I'm a propertyInfo");

            var actual = printer.PrintToString(person);

            actual.Should().BeEquivalentTo(
                "Person" +
                "\r\n\tId = 00000001-0002-0003-0405-060708090a0b" +
                "\r\n\tName = Alex" +
                "\r\n\tHeight = I'm a propertyInfo" +
                "\r\n\tAge = 19" +
                "\r\n\tSurname = Alexovich" +
                "\r\n");
        }

        [Test]
        public void Using_ShouldUseSelectedCulture()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(CultureInfo.InvariantCulture);

            var actual = printer.PrintToString(person);

            actual.Should().BeEquivalentTo(
                "Person" +
                "\r\n\tId = 00000001-0002-0003-0405-060708090a0b" +
                "\r\n\tName = Alex" +
                "\r\n\tHeight = 70.5" +
                "\r\n\tAge = 19" +
                "\r\n\tSurname = Alexovich" +
                "\r\n");
        }

        [Test]
        public void Using_ShouldUseCustomDeserializingForProperty()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(x => x.Name).Using(_ => "It's my name");

            var actual = printer.PrintToString(person);

            actual.Should().BeEquivalentTo(
                "Person" +
                "\r\n\tId = 00000001-0002-0003-0405-060708090a0b" +
                "\r\n\tName = It's my name" +
                "\r\n\tHeight = 70,5" +
                "\r\n\tAge = 19" +
                "\r\n\tSurname = Alexovich" +
                "\r\n");
        }

        [Test]
        public void RestrictStringFor_ShouldRestrictStringsForLength()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<string>().TrimmedToLength(2);

            var actual = printer.PrintToString(person);

            actual.Should().BeEquivalentTo(
                "Person" +
                "\r\n\tId = 00000001-0002-0003-0405-060708090a0b" +
                "\r\n\tName = Al" +
                "\r\n\tHeight = 70,5" +
                "\r\n\tAge = 19" +
                "\r\n\tSurname = Al" +
                "\r\n");
        }

        [Test]
        public void Exclude_ShouldExcludeSelectedProperty()
        {
            var printer = ObjectPrinter.For<Person>().Excluding(x => x.Name);

            var actual = printer.PrintToString(person);

            actual.Should().BeEquivalentTo(
                "Person" +
                "\r\n\tId = 00000001-0002-0003-0405-060708090a0b" +
                "\r\n\tHeight = 70,5" +
                "\r\n\tAge = 19" +
                "\r\n\tSurname = Alexovich" +
                "\r\n");
        }

        [Test]
        public void Exclude_ShouldExcludeSelectedTypeFromNestingClasses()
        {
            var printer = ObjectPrinter.For<PersonWithParent>().Excluding<int>();

            var actual = printer.PrintToString(personWithParent);

            actual.Should().BeEquivalentTo(
                "PersonWithParent" +
                "\r\n\tId = 00000001-0002-0003-0405-060708090a0b" +
                "\r\n\tName = Alex" +
                "\r\n\tHeight = 70,5" +
                "\r\n\tParent = PersonWithParent" +
                "\r\n\t\tId = 0000000b-000a-0009-0807-060504030201" +
                "\r\n\t\tName = Kostya" +
                "\r\n\t\tHeight = 80" +
                "\r\n\t\tParent = null" +
                "\r\n\t\tSurname = null" +
                "\r\n\tSurname = Seshovich" +
                "\r\n");
        }

        [Test]
        public void Using_ShouldWorkCorrectly_AfterSetCustomDeserializationForProperty()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).Using(_ => "Hi")
                .Printing<int>().Using(_ => "Int");

            var actual = printer.PrintToString(person);

            actual.Should().BeEquivalentTo("Person" +
                                           "\r\n\tId = 00000001-0002-0003-0405-060708090a0b" +
                                           "\r\n\tName = Hi" +
                                           "\r\n\tHeight = 70,5" +
                                           "\r\n\tAge = Int" +
                                           "\r\n\tSurname = Alexovich" +
                                           "\r\n");
        }

        [Test]
        public void PrintToString_ShouldNotThrowStackOverflowException_WhenObjectHasCycleLink()
        {
            var persons = GetPersonsWithParentWithCycleLink();
            Action act = () => persons[0].PrintToString();

            act.Should().NotThrow<StackOverflowException>();
        }

        [Test]
        public void PrintToString_ShouldThrowArgumentException_WhenMaxStringLengthIsNegative()
        {
            Action act = () => person.PrintToString(x => x.Printing<string>().TrimmedToLength(-1));

            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void PrintToString_ShouldPrintPropertiesOnce_WhenObjectHasCycleLink()
        {
            var persons = GetPersonsWithParentWithCycleLink();

            var actual = persons[0].PrintToString();

            actual.Should().BeEquivalentTo(
                "PersonWithParent" +
                "\r\n\tId = 00000001-0002-0003-0405-060708090a0b" +
                "\r\n\tName = Alex" +
                "\r\n\tHeight = 70,5" +
                "\r\n\tAge = 19" +
                "\r\n\tParent = PersonWithParent" +
                "\r\n\t\tId = 0000000b-000a-0009-0807-060504030201" +
                "\r\n\t\tName = Kostya" +
                "\r\n\t\tHeight = 80" +
                "\r\n\t\tAge = 28" +
                "\r\n\t\tParent = This object was printed already" +
                "\r\n\t\tSurname = Seshovich" +
                "\r\n\tSurname = Seshovich" +
                "\r\n");
        }

        [Test]
        public void PrintToString_ShouldPrintPropertyWithTypeOfList()
        {
            var actual = objectWithList.PrintToString();

            actual.Should().BeEquivalentTo("ObjectWithEnumerable" +
                                           "\r\n\tCollection = IEnumerable" +
                                           "\r\n\t\t1" +
                                           "\r\n\t\t2" +
                                           "\r\n\t\t3" +
                                           "\r\n");
        }

        [Test]
        public void PrintToString_ShouldPrintPropertyWithTypeOfHashSet()
        {
            var actual = objectWithHashSet.PrintToString();

            actual.Should().BeEquivalentTo("ObjectWithEnumerable" +
                                           "\r\n\tCollection = IEnumerable" +
                                           "\r\n\t\t1" +
                                           "\r\n\t\t2" +
                                           "\r\n\t\t3" +
                                           "\r\n");
        }

        [Test]
        public void PrintToString_ShouldPrintPropertyWithTypeOfDictionary()
        {
            var actual = objectWithDictionary.PrintToString();

            actual.Should().BeEquivalentTo("ObjectWithDictionary\r\n\tDictionary = IDictionary" +
                                           "\r\n\t\tOne : 1" +
                                           "\r\n\t\tTwo : 2" +
                                           "\r\n\t\tThree : 3" +
                                           "\r\n");
        }
    }
}