using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Extensions;
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
                Id = new Guid(11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1),
                Parent = null,
                Surname = null
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

        private readonly ObjectWithDictionary objectWithDictionary = new()
        {
            Dictionary = new Dictionary<string, int>
            {
                { "One", 1 },
                { "Two", 2 },
                { "Three", 3 }
            }
        };

        private static IEnumerable<TestCaseData> GetTestsWithObjectsWithEnumerable()
        {
            yield return new TestCaseData(
                new ObjectWithEnumerable
                {
                    Collection = new List<int> { 1, 2, 3 }
                }).SetName("List case");

            yield return
                new TestCaseData(new ObjectWithEnumerable
                {
                    Collection = new HashSet<int> { 1, 2, 3 }
                }).SetName("HashSet case");

            yield return
                new TestCaseData(new ObjectWithEnumerable
                {
                    Collection = new[] { 1, 2, 3 }
                }).SetName("Array case");
        }

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

        private PersonWithTwoParents GetPersonWithTwoParentsReferenceAtOneObject()
        {
            var personWithTwoParents = new PersonWithTwoParents
            {
                Name = "Alex",
                FirstParent = new PersonWithTwoParents
                {
                    Name = "Kostya"
                },
                SecondParent = new PersonWithTwoParents
                {
                    Name = "Nastya"
                }
            };

            var sharedParent = new PersonWithTwoParents
            {
                Name = "General"
            };

            personWithTwoParents.FirstParent.FirstParent = sharedParent;
            personWithTwoParents.SecondParent.FirstParent = sharedParent;

            return personWithTwoParents;
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
                                           $"\r\n\tId = {person.Id.ToString()}" +
                                           $"\r\n\tName = {person.Name}" +
                                           $"\r\n\tHeight = {person.Height}" +
                                           $"\r\n\tSurname = {person.Surname}" +
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
                $"\r\n\tId = {person.Id.GetStringValueOrDefault()}" +
                $"\r\n\tName = {person.Name.GetStringValueOrDefault()}" +
                "\r\n\tHeight = I'm a propertyInfo" +
                $"\r\n\tAge = {person.Age.GetStringValueOrDefault()}" +
                $"\r\n\tSurname = {person.Surname.GetStringValueOrDefault()}" +
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
                $"\r\n\tId = {person.Id.GetStringValueOrDefault()}" +
                $"\r\n\tName = {person.Name.GetStringValueOrDefault()}" +
                $"\r\n\tHeight = {person.Height.GetStringValueOrDefault(CultureInfo.InvariantCulture)}" +
                $"\r\n\tAge = {person.Age.GetStringValueOrDefault()}" +
                $"\r\n\tSurname = {person.Surname.GetStringValueOrDefault()}" +
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
                $"\r\n\tId = {person.Id.GetStringValueOrDefault()}" +
                "\r\n\tName = It's my name" +
                $"\r\n\tHeight = {person.Height.GetStringValueOrDefault()}" +
                $"\r\n\tAge = {person.Age.GetStringValueOrDefault()}" +
                $"\r\n\tSurname = {person.Surname.GetStringValueOrDefault()}" +
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
                $"\r\n\tId = {person.Id.GetStringValueOrDefault()}" +
                $"\r\n\tName = {person.Name.GetStringValueOrDefault()[..Math.Min(2, person.Name.Length)]}" +
                $"\r\n\tHeight = {person.Height.GetStringValueOrDefault()}" +
                $"\r\n\tAge = {person.Age.GetStringValueOrDefault()}" +
                $"\r\n\tSurname = {person.Surname.GetStringValueOrDefault()[..Math.Min(2, person.Surname.Length)]}" +
                "\r\n");
        }

        [Test]
        public void Exclude_ShouldExcludeSelectedProperty()
        {
            var printer = ObjectPrinter.For<Person>().Excluding(x => x.Name);

            var actual = printer.PrintToString(person);

            actual.Should().BeEquivalentTo(
                "Person" +
                $"\r\n\tId = {person.Id.GetStringValueOrDefault()}" +
                $"\r\n\tHeight = {person.Height.GetStringValueOrDefault()}" +
                $"\r\n\tAge = {person.Age.GetStringValueOrDefault()}" +
                $"\r\n\tSurname = {person.Surname.GetStringValueOrDefault()}" +
                "\r\n");
        }

        [Test]
        public void Exclude_ShouldExcludeSelectedTypeFromNestingClasses()
        {
            var printer = ObjectPrinter.For<PersonWithParent>().Excluding<int>();

            var actual = printer.PrintToString(personWithParent);

            actual.Should().BeEquivalentTo(
                "PersonWithParent" +
                $"\r\n\tId = {personWithParent.Id.GetStringValueOrDefault()}" +
                $"\r\n\tName = {personWithParent.Name.GetStringValueOrDefault()}" +
                $"\r\n\tHeight = {personWithParent.Height.GetStringValueOrDefault()}" +
                "\r\n\tParent = PersonWithParent" +
                $"\r\n\t\tId = {personWithParent.Parent.Id.GetStringValueOrDefault()}" +
                $"\r\n\t\tName = {personWithParent.Parent.Name.GetStringValueOrDefault()}" +
                $"\r\n\t\tHeight = {personWithParent.Parent.Height.GetStringValueOrDefault()}" +
                $"\r\n\t\tParent = {personWithParent.Parent.Parent.GetStringValueOrDefault()}" +
                $"\r\n\t\tSurname = {personWithParent.Parent.Surname.GetStringValueOrDefault()}" +
                $"\r\n\tSurname = {personWithParent.Surname.GetStringValueOrDefault()}" +
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
                                           $"\r\n\tId = {person.Id.GetStringValueOrDefault()}" +
                                           "\r\n\tName = Hi" +
                                           $"\r\n\tHeight = {person.Height.GetStringValueOrDefault()}" +
                                           "\r\n\tAge = Int" +
                                           $"\r\n\tSurname = {person.Surname.GetStringValueOrDefault()}" +
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
                $"\r\n\tId = {persons[0].Id.GetStringValueOrDefault()}" +
                $"\r\n\tName = {persons[0].Name.GetStringValueOrDefault()}" +
                $"\r\n\tHeight = {persons[0].Height.GetStringValueOrDefault()}" +
                $"\r\n\tAge = {persons[0].Age.GetStringValueOrDefault()}" +
                "\r\n\tParent = PersonWithParent" +
                $"\r\n\t\tId = {persons[1].Id.GetStringValueOrDefault()}" +
                $"\r\n\t\tName = {persons[1].Name.GetStringValueOrDefault()}" +
                $"\r\n\t\tHeight = {persons[1].Height.GetStringValueOrDefault()}" +
                $"\r\n\t\tAge = {persons[1].Age.GetStringValueOrDefault()}" +
                "\r\n\t\tParent = This object was printed already" +
                $"\r\n\t\tSurname = {persons[1].Surname.GetStringValueOrDefault()}" +
                $"\r\n\tSurname = {persons[0].Surname.GetStringValueOrDefault()}" +
                "\r\n");
        }

        [TestCaseSource(nameof(GetTestsWithObjectsWithEnumerable))]
        public void PrintToString_ShouldPrintPropertyWithTypeOfIEnumerable(ObjectWithEnumerable objectWithEnumerable)
        {
            var actual = objectWithEnumerable.PrintToString();

            actual.Should().BeEquivalentTo("ObjectWithEnumerable" +
                                           "\r\n\tCollection = IEnumerable" +
                                           objectWithEnumerable.Collection
                                               .GetStringValueOrDefault(new string('\t', 2)));
        }

        [Test]
        public void PrintToString_ShouldPrintPropertyWithTypeOfDictionary()
        {
            var actual = objectWithDictionary.PrintToString();

            actual.Should().BeEquivalentTo("ObjectWithDictionary\r\n\tDictionary = IDictionary" +
                                           objectWithDictionary.Dictionary
                                               .GetStringValueOrDefault(new string('\t', 2)));
        }

        [Test]
        public void PrintToString_ShouldNotPrintObjectTwice()
        {
            var personWithSameNestingObject = GetPersonWithTwoParentsReferenceAtOneObject();

            var actual = personWithSameNestingObject.PrintToString();

            actual.Should()
                .BeEquivalentTo("PersonWithTwoParents" +
                                $"\r\n\tName = {personWithSameNestingObject.Name.GetStringValueOrDefault()}" +
                                "\r\n\tFirstParent = PersonWithTwoParents" +
                                $"\r\n\t\tName = {personWithSameNestingObject.FirstParent.Name.GetStringValueOrDefault()}" +
                                "\r\n\t\tFirstParent = PersonWithTwoParents" +
                                $"\r\n\t\t\tName = {personWithSameNestingObject.FirstParent.FirstParent.Name.GetStringValueOrDefault()}" +
                                $"\r\n\t\t\tFirstParent = {personWithSameNestingObject.FirstParent.FirstParent.FirstParent.GetStringValueOrDefault()}" +
                                $"\r\n\t\t\tSecondParent = {personWithSameNestingObject.FirstParent.FirstParent.SecondParent.GetStringValueOrDefault()}" +
                                "\r\n\t\tSecondParent = null" +
                                "\r\n\tSecondParent = PersonWithTwoParents" +
                                $"\r\n\t\tName = {personWithSameNestingObject.SecondParent.Name.GetStringValueOrDefault()}" +
                                "\r\n\t\tFirstParent = This object was printed already" +
                                "\r\n\t\tSecondParent = null" +
                                "\r\n");
        }

        [Test]
        public void PrintToString_ShouldPrintNestingCollectionCorrectly()
        {
            var objectWithNestingList = new ObjectWithNestingEnumerable
            {
                Collection = new List<List<int>>
                {
                    new() { 1, 2, 3 },
                    new() { 4, 5, 6 },
                    new() { 7, 8, 9 }
                }
            };

            var actual = objectWithNestingList.PrintToString();

            actual.Should().BeEquivalentTo("ObjectWithNestingEnumerable" +
                                           "\r\n\tCollection = IEnumerable" +
                                           objectWithNestingList.Collection
                                               .GetStringValueOrDefault(
                                                   new string('\t', 2),
                                                   new string('\t', 3)));
        }
    }
}