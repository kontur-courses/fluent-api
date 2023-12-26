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

        [SetUp]
        public void SetUp()
        {
            sut = new PrintingConfig<Person>();
            simplePerson = new Person {Name = "Alex", Age = 19};
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
    }
}