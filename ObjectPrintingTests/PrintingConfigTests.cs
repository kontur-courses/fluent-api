using System;
using System.Globalization;
using NUnit.Framework;
using ObjectPrinting.Extensions;
using FluentAssertions;

namespace ObjectPrinting.Tests
{
    public class PrintingConfigTests
    {
        private PrintingConfig<Person> sut;
        private Person simplePerson;
        private Person personWithNesting;
        private Person personWithCyclicReference;
        private List<Person> persons;
        private Dictionary<string, Person> dictPersons;

        [SetUp]
        public void SetUp()
        {
            sut = new PrintingConfig<Person>();
            simplePerson = new Person {Name = "Alex", Surname = "Obama", Age = 19};
            personWithNesting = new Person {Name = "Oleg", Surname = "Ponso", Age = 5, Parent = simplePerson};
            personWithCyclicReference = new Person {Name = "IAmCyclic", Surname = "ThatsHorrible", Age = 19};
            personWithCyclicReference.Parent = personWithCyclicReference;
            persons = new List<Person> {simplePerson, personWithNesting};
            dictPersons = new Dictionary<string, Person>
            {
                [simplePerson.Name] = simplePerson,
                [personWithNesting.Name] = personWithNesting
            };
        }

        [Test]
        public void Printing_ThrowsException_OnProvidingNotMemberExpression()
        {
            var a = () => sut.Printing(x => x);

            a.Should().Throw<ArgumentException>();
        }
        
        [Test]
        public void Printing_ThrowsException_OnAccessingSomethingNotDeclaredInOwnedClass()
        {
            var differentObj = (1, 2);
            var a = () => sut.Printing(x => differentObj.Item1);

            a.Should().Throw<ArgumentException>();
        }

        [Test]
        public void Printing_NotThrowsStackOverflowException_OnPrintingObjectWithCyclicReference()
        {
            var a = () => sut.PrintToString(personWithCyclicReference);

            a.Should().NotThrow<StackOverflowException>();
        }
        
        [Test]
        public void Printing_ReturnsStringWithAllFieldAndProperties_OnDefault()
        {
            sut.PrintToString(simplePerson)
                .Should()
                .ContainAll(nameof(Person.Age), nameof(Person.Id), nameof(Person.Height), nameof(Person.Name),
                    nameof(Person.Surname), nameof(Person.Parent))
                .And.ContainAll(
                    simplePerson.Name, 
                    simplePerson.Height.ToString(CultureInfo.InvariantCulture), 
                    simplePerson.Age.ToString(), 
                    simplePerson.Id.ToString(),
                    "null", 
                    simplePerson.Surname);
        }

        [Test]
        public void PrintToString_WorksCorrect_OnCollections()
        {
            persons.PrintToString()
                .Should()
                .ContainAll(simplePerson.Name, personWithNesting.Name, personWithNesting.Parent.Name)
                .And.ContainAll("[0]:", "[1]:");
            
            dictPersons.PrintToString()
                .Should()
                .ContainAll(simplePerson.Name, personWithNesting.Name, personWithNesting.Parent.Name)
                .And.ContainAll($"[{simplePerson.Name}]:", $"[{personWithNesting.Name}]:");
        }
        
        [Test]
        public void Excluding_DoNotAffectsPreviousConfig_OnExcludeType()
        {
            var first = sut.Excluding<string>();
            var second = first.Excluding<Guid>();
            first.PrintToString(simplePerson).Should().NotBeSameAs(second.PrintToString(simplePerson));
        }
        
        [Test]
        public void Excluding_DoNotAffectsPreviousConfig_OnExcludeMember()
        {
            var first = sut.Excluding(x => x.Age);
            var second = first.Excluding(x => x.Id);
            first.PrintToString(simplePerson).Should().NotBeSameAs(second.PrintToString(simplePerson));
        }
        
        [Test]
        public void Using_DoNotAffectsPreviousConfig_OnProvidingTypeSerializer()
        {
            var first = sut.Printing<int>().Using(x => "X");
            var second = first.Printing<int>().Using(x => "Y");
            first.PrintToString(simplePerson).Should().NotBeSameAs(second.PrintToString(simplePerson));
        }
        
        [Test]
        public void Using_DoNotAffectsPreviousConfig_OnProvidingMemberSerializer()
        {
            var first = sut.Printing(x => x.Id).Using(x => "X");
            var second = first.Printing(x => x.Id).Using(x => "Y");
            first.PrintToString(simplePerson).Should().NotBeSameAs(second.PrintToString(simplePerson));
        }
        
        [Test]
        public void Excluding_ExcludeTypeMembersFromString_OnExcludeType()
        {
            sut.Excluding<string>().PrintToString(simplePerson).Should().NotContainAny("Name");
            sut.Excluding<Guid>().PrintToString(simplePerson).Should().NotContainAny("Id");
            sut.Excluding<int>().PrintToString(simplePerson).Should().NotContainAny("Age");
        }
        
        [Test]
        public void Excluding_ExcludeTypeMembersFromNestingString_OnExcludeType()
        {
            sut.Excluding<string>().PrintToString(personWithNesting).Should().NotContainAny("Name");
            sut.Excluding<Guid>().PrintToString(personWithNesting).Should().NotContainAny("Id");
            sut.Excluding<int>().PrintToString(personWithNesting).Should().NotContainAny("Age");
        }
        
        [Test]
        public void Excluding_ExcludesMembersFromString_OnExcludeMember()
        {
            sut.Excluding(x => x.Age).PrintToString(simplePerson).Should().NotContainAny("Age");
            sut.Excluding(x => x.Id).PrintToString(simplePerson).Should().NotContainAny("Id");
            sut.Excluding(x => x.Name).PrintToString(simplePerson).Should().NotContainAny("Name");
        }
        
        [Test]
        public void Excluding_ExcludesMembersFromNestingString_OnExcludeMember()
        {
            sut.Excluding(x => x.Age).PrintToString(personWithNesting).Should().NotContainAny("Age");
            sut.Excluding(x => x.Id).PrintToString(personWithNesting).Should().NotContainAny("Id");
            sut.Excluding(x => x.Name).PrintToString(personWithNesting).Should().NotContainAny("Name");
        }
        
        [Test]
        public void Using_AppliedCultureToMembers_OnProvidingCultureToMember()
        {
            var ruCulture = CultureInfo.GetCultureInfo("ru");
            
            sut.Printing(x => x.Height)
                .Using(CultureInfo.InvariantCulture)
                .PrintToString(simplePerson)
                .Should()
                .Contain(simplePerson.Height.ToString(CultureInfo.InvariantCulture));
            
            sut.Printing(x => x.Height)
                .Using(ruCulture)
                .PrintToString(simplePerson)
                .Should()
                .Contain(simplePerson.Height.ToString(ruCulture));
        }
        
        [Test]
        public void Using_AppliedCultureToTypes_OnProvidingCultureToType()
        {
            var ruCulture = CultureInfo.GetCultureInfo("ru");
       
            sut.Printing<double>()
                .Using(CultureInfo.InvariantCulture)
                .PrintToString(simplePerson)
                .Should()
                .Contain(simplePerson.Height.ToString(CultureInfo.InvariantCulture));
            
            sut.Printing<double>()
                .Using(ruCulture)
                .PrintToString(simplePerson)
                .Should()
                .Contain(simplePerson.Height.ToString(ruCulture));
        }
        
        [Test]
        public void Using_AppliesCustomTypeSerialize_OnProvidingTypeSerializer()
        {
            sut.Printing<int>()
                .Using(x => x > 18 ? "Старый" : "Молодой")
                .PrintToString(simplePerson)
                .Should()
                .ContainAny("Старый")
                .And
                .NotContainAny("Молодой");
        }
        
        [Test]
        public void Using_AppliesCustomMemberSerialize_OnProvidingMemberSerializer()
        {
            sut.Printing(x => x.Age)
                .Using(x => x > 18 ? "Старый" : "Молодой")
                .PrintToString(simplePerson)
                .Should()
                .ContainAny("Старый")
                .And
                .NotContainAny("Молодой");
        }

        [Test]
        public void TrimmedToLength_TrimsStrings_OfProvidedType()
        {
            sut.Printing<string>()
                .TrimmedToLength(2)
                .PrintToString(simplePerson)
                .Should()
                .Contain("Al")
                .And.Contain("Ob")
                .And.NotContain("Alex")
                .And.NotContain("Obama");
        }
        
        [Test]
        public void TrimmedToLength_TrimsStrings_OfProvidedMember()
        {
            sut.Printing(x => x.Name)
                .TrimmedToLength(2)
                .PrintToString(simplePerson)
                .Should()
                .Contain("Al")
                .And.NotContain("Alex")
                .And.Contain("Obama");
        }
    }
}