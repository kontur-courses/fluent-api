using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public static class PrintingConfig_Should
    {
        [Test]
        public static void PrintBasicProperties()
        {
            var persons = GetRandomBasicPersons();
            foreach (var person in persons)
            {
                var print = person.PrintToString();
                
                print.Should().Contain(nameof(Person));
                print.Should().Contain($"\n\t{nameof(Person.Name)} = {person.Name}");
                print.Should().Contain($"\n\t{nameof(Person.Age)} = {person.Age}");
                print.Should().Contain($"\n\t{nameof(Person.Height)} = {person.Height}");
            }
        }
        
        [Test]
        public static void PrintBasicFields_WhenFieldsNull()
        {
            var persons = GetRandomBasicPersons();
            foreach (var person in persons)
            {
                var print = person.PrintToString();
                
                print.Should().Contain(nameof(Person));
                print.Should().Contain($"\n\t{nameof(person.Father)} = null");
                print.Should().Contain($"\n\t{nameof(person.Mother)} = null");
                print.Should().Contain($"\n\t{nameof(person.SomeArray)} = null");
                print.Should().Contain($"\n\t{nameof(person.SomeDict)} = null");
            }
        }
        
        [Test]
        public static void AllowExcludeTypes()
        {
            var persons = GetRandomBasicPersons();
            foreach (var person in persons)
            {
                var print = person.PrintToString();
                var printExcluded = person.PrintToString(c => c
                    .Excluding<Guid>()
                    .Excluding<double>()
                    .Excluding<DateTime>());

                print.Should().Contain($"\n\t{nameof(Person.Id)} = ");
                print.Should().Contain($"\n\t{nameof(Person.Height)} = ");
                print.Should().Contain($"\n\t{nameof(Person.BirthDate)} = ");
                
                printExcluded.Should().NotContain($"\n\t{nameof(Person.Id)} = ");
                printExcluded.Should().NotContain($"\n\t{nameof(Person.Height)} = ");
                printExcluded.Should().NotContain($"\n\t{nameof(Person.BirthDate)} = ");
            }
        }
        
        [Test]
        public static void AllowExcludeSpecificMembers()
        {
            var persons = GetRandomBasicPersons();
            foreach (var person in persons)
            {
                var print = person.PrintToString();
                var printExcluded = person.PrintToString(c => c
                    .Excluding(p => p.Id)
                    .Excluding(p => p.Height)
                    .Excluding(p => p.BirthDate));

                print.Should().Contain($"\n\t{nameof(Person.Id)} = ");
                print.Should().Contain($"\n\t{nameof(Person.Height)} = ");
                print.Should().Contain($"\n\t{nameof(Person.BirthDate)} = ");
                
                printExcluded.Should().NotContain($"\n\t{nameof(Person.Id)} = ");
                printExcluded.Should().NotContain($"\n\t{nameof(Person.Height)} = ");
                printExcluded.Should().NotContain($"\n\t{nameof(Person.BirthDate)} = ");
            }
        }
        
        [Test]
        public static void PrintObject_WithRecursiveMembers()
        {
            var persons = GetRandomBasicPersons().WithRandomParents(3);
            foreach (var person in persons)
            {
                var print = person.PrintToString();
                TestForParents(print, person);
            }

            void TestForParents(string print, Person person, int recursionLevel = 0, int maxRecursionLevel = 3)
            {
                if (recursionLevel > maxRecursionLevel) return;
                var identation = new string('\t', recursionLevel + 1);
                
                print.Should().Contain($"\n{identation}{nameof(Person.Name)} = {person.Name}");
                print.Should().Contain($"\n{identation}{nameof(Person.Age)} = {person.Age}");
                print.Should().Contain($"\n{identation}{nameof(Person.Height)} = {person.Height}");
                
                TestForParents(print, person.Father, recursionLevel + 1);
                TestForParents(print, person.Mother, recursionLevel + 1);
            }
        }

        [Test]
        public static void AllowCustomPrinting_ForTypes()
        {
            var persons = GetRandomBasicPersons();
            foreach (var person in persons)
            {
                var print = person.PrintToString();
                var customPrint = person.PrintToString(c => c
                    .Printing<Guid>().As(g => g.ToString())
                    .Printing<DateTime>().As(d => $"year: {d.Year}"));

                print.Should().NotContain($"\n\t{nameof(Person.Id)} = {person.Id.ToString()}");
                print.Should().NotContain($"\n\t{nameof(Person.BirthDate)} = year: {person.BirthDate.Year}");
                
                customPrint.Should().Contain($"\n\t{nameof(Person.Id)} = {person.Id.ToString()}");
                customPrint.Should().Contain($"\n\t{nameof(Person.BirthDate)} = year: {person.BirthDate.Year}");
            }
        }
        
        [Test]
        public static void AllowCustomPrinting_ForMembers()
        {
            var persons = GetRandomBasicPersons();
            foreach (var person in persons)
            {
                var print = person.PrintToString();
                var customPrint = person.PrintToString(c => c
                    .Printing(p => p.Id).As(g => g.ToString())
                    .Printing(p => p.BirthDate).As(d => $"year: {d.Year}"));

                print.Should().NotContain($"\n\t{nameof(Person.Id)} = {person.Id.ToString()}");
                print.Should().NotContain($"\n\t{nameof(Person.BirthDate)} = year: {person.BirthDate.Year}");
                
                customPrint.Should().Contain($"\n\t{nameof(Person.Id)} = {person.Id.ToString()}");
                customPrint.Should().Contain($"\n\t{nameof(Person.BirthDate)} = year: {person.BirthDate.Year}");
            }
        }
        
        [Test]
        public static void AllowCustomPrinting_ForTypes_WithRecursiveMembers()
        {
            var persons = GetRandomBasicPersons().WithRandomParents(3).WithRandomId();
            foreach (var person in persons)
            {
                var print = person.PrintToString();
                var customPrint = person.PrintToString(c => c
                    .Printing<Guid>().As(g => g.ToString())
                    .Printing<DateTime>().As(d => $"year: {d.Year}"));
                TestForParents(print, customPrint, person);
            }

            void TestForParents(string print, string customPrint, Person person, int recursionLevel = 0, int maxRecursionLevel = 3)
            {
                if (recursionLevel > maxRecursionLevel) return;
                var identation = new string('\t', recursionLevel + 1);
                
                print.Should().NotContain($"\n{identation}{nameof(Person.Id)} = {person.Id.ToString()}");
                print.Should().NotContain($"\n{identation}{nameof(Person.BirthDate)} = year: {person.BirthDate.Year}");
                
                customPrint.Should().Contain($"\n{identation}{nameof(Person.Id)} = {person.Id.ToString()}");
                customPrint.Should().Contain($"\n{identation}{nameof(Person.BirthDate)} = year: {person.BirthDate.Year}");
                
                TestForParents(print, customPrint, person.Father, recursionLevel + 1);
                TestForParents(print, customPrint, person.Mother, recursionLevel + 1);
            }
        }
        
        [Test]
        public static void AllowCustomPrinting_ForDeepMembers()
        {
            var persons = GetRandomBasicPersons().WithRandomParents(3);
            foreach (var person in persons)
            {
                var print = person.PrintToString();
                var customPrint = person.PrintToString(c => c
                    .Printing(p => p.Father.Mother.Father.Id).As(g => g.ToString())
                    .Printing(p => p.Mother.Father.Mother.BirthDate).As(d => $"year: {d.Year}"));

                print.Should().NotContain(
                    $"\n\t\t\t\t{nameof(Person.Id)} = {person.Father.Mother.Father.Id.ToString()}");
                print.Should().NotContain(
                    $"\n\t\t\t\t{nameof(Person.BirthDate)} = year: {person.Mother.Father.Mother.BirthDate.Year}");
                
                customPrint.Should().Contain(
                    $"\n\t\t\t\t{nameof(Person.Id)} = {person.Father.Mother.Father.Id.ToString()}");
                customPrint.Should().Contain(
                    $"\n\t\t\t\t{nameof(Person.BirthDate)} = year: {person.Mother.Father.Mother.BirthDate.Year}");
            }
        }

        [Test]
        public static void AllowCustomCulture()
        {
            var persons = GetRandomBasicPersons().WithNotRoundHeight();
            foreach (var person in persons)
            {
                var print = person.PrintToString();
                var customCultures = new[]
                {
                    CultureInfo.CurrentCulture,
                    CultureInfo.InvariantCulture,
                    CultureInfo.CreateSpecificCulture("en-US"),
                    CultureInfo.CreateSpecificCulture("nl-NL")
                };

                print.Should()
                    .Contain($"\n\t{nameof(Person.Height)} = {person.Height.ToString(CultureInfo.CurrentCulture)}");
                foreach (var customCulture in customCultures)
                {
                    var customPrint = person.PrintToString(c => c
                        .Printing<double>().Using(customCulture));
                    customPrint.Should()
                        .Contain($"\n\t{nameof(Person.Height)} = {person.Height.ToString(customCulture)}");
                }
            }
        }
        
        [Test]
        public static void AllowCustomFormats()
        {
            var persons = GetRandomBasicPersons().WithNotRoundHeight();
            foreach (var person in persons)
            {
                var print = person.PrintToString();
                var customFormats = new[]
                {
                    "f5",
                    "F10",
                    "C"
                };

                print.Should()
                    .Contain($"\n\t{nameof(Person.Height)} = {person.Height.ToString(null, CultureInfo.CurrentCulture)}");
                foreach (var customFormat in customFormats)
                {
                    var customPrint = person.PrintToString(c => c
                        .Printing<double>().Using(customFormat));
                    customPrint.Should()
                        .Contain($"\n\t{nameof(Person.Height)} = {person.Height.ToString(customFormat)}");
                }
            }
        }

        [Test]
        public static void PrintArrays()
        {
            var persons = GetRandomBasicPersons().WithRandomArray();
            foreach (var person in persons)
            {
                var print = person.PrintToString().Split('\n').ToList();

                print.Should().Contain(s => s.Contains($"{nameof(Person.SomeArray)} = "));
                var line = print.FindIndex(s => s.Contains($"{nameof(Person.SomeArray)} = ")) + 1;
                for (var i = 0; i < person.SomeArray.Length; i++)
                    print[line + i].Should().Contain(person.SomeArray[i].ToString());
            }
        }
        
        [Test]
        public static void PrintDictionaries()
        {
            var persons = GetRandomBasicPersons().WithRandomDictionary();
            foreach (var person in persons)
            {
                var print = person.PrintToString();
                print.Should().Contain($"\n\t{nameof(Person.SomeDict)} = ");
                foreach (var (key, value) in person.SomeDict)
                    print.Should().Contain($"\n\t\t{key}: {value}");
            }
        }

        public static IEnumerable<Person> GetRandomBasicPersons(int count = 100)
        {
            for (var i = 0; i < count; i++)
                yield return GetRandomBasicPerson();
        }

        public static Person GetRandomBasicPerson()
        {
            var random = new Random();
            var randomNames = new[] {"Aaron", "Alex", "Bob", "John", "Max", "Dilan", "Bud"};
            
            return new Person
            {
                Name = randomNames[random.Next(randomNames.Length)],
                Age = random.Next(10, 80),
                Height = random.Next(80, 180),
                SomeArray = null,
                SomeDict = null,
                Mother = null,
                Father = null
            };
        }

        public static IEnumerable<Person> WithRandomParents(this IEnumerable<Person> persons, int maxRecursionLevel = 1)
            => persons.Select(p => p.WithRandomParents(maxRecursionLevel));

        public static Person WithRandomParents(this Person person, int maxRecursionLevel = 1)
        {
            if (maxRecursionLevel == 0) return person;
            var father = GetRandomBasicPerson().WithRandomParents(maxRecursionLevel - 1);
            var mother = GetRandomBasicPerson().WithRandomParents(maxRecursionLevel - 1);
            person.Father = father;
            person.Mother = mother;
            return person;
        }

        public static IEnumerable<Person> WithRandomId(this IEnumerable<Person> persons)
            => persons.Select(WithRandomId);

        public static Person WithRandomId(this Person person)
        {
            person.Id = Guid.NewGuid();
            return person;
        }

        public static IEnumerable<Person> WithNotRoundHeight(this IEnumerable<Person> persons)
            => persons.Select(WithNotRoundHeight);

        public static Person WithNotRoundHeight(this Person person)
        {
            person.Height += new Random().NextDouble();
            return person;
        }

        public static IEnumerable<Person> WithRandomArray(this IEnumerable<Person> persons)
            => persons.Select(WithRandomArray);
        
        public static Person WithRandomArray(this Person person)
        {
            var random = new Random();
            var length = random.Next(10);
            person.SomeArray = new int[length];
            for (var i = 0; i < length; i++)
                person.SomeArray[i] = random.Next(1000);
            return person;
        }
        
        public static IEnumerable<Person> WithRandomDictionary(this IEnumerable<Person> persons)
            => persons.Select(WithRandomDictionary);
        
        public static Person WithRandomDictionary(this Person person)
        {
            var keys = new[] {"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n"};
            var random = new Random();
            var length = random.Next(10);
            person.SomeDict = new Dictionary<string, int>();
            for (var i = 0; i < length; i++)
                person.SomeDict[keys[random.Next(keys.Length)]] = random.Next(1000);
            return person;
        }
    }
}