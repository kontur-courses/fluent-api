using System;
using System.Collections.Generic;
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

            void TestForParents(string print, Person person, int currentRecursionLevel = 0, int maxRecursionLevel = 3)
            {
                if (currentRecursionLevel > maxRecursionLevel) return;
                var identation = new string('\t', currentRecursionLevel + 1);
                
                print.Should().Contain($"\n{identation}{nameof(Person.Name)} = {person.Name}");
                print.Should().Contain($"\n{identation}{nameof(Person.Age)} = {person.Age}");
                print.Should().Contain($"\n{identation}{nameof(Person.Height)} = {person.Height}");
                
                TestForParents(print, person.Father, currentRecursionLevel + 1);
                TestForParents(print, person.Mother, currentRecursionLevel + 1);
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
    }
}