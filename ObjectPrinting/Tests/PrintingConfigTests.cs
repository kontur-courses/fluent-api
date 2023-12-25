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

        [SetUp]
        public void SetUp()
        {
            sut = new PrintingConfig<Person>();
        }
        
        [Test]
        public void PrintToString_ExcludeTypeMembersFromString_OnExcludeType()
        {
            var person = new Person {Name = "Alex", Age = 19};

            sut.Excluding<string>().PrintToString(person).Should().NotContainAny("Name");
            sut.Excluding<Guid>().PrintToString(person).Should().NotContainAny("Id");
            sut.Excluding<int>().PrintToString(person).Should().NotContainAny("Age");
        }
        
        [Test]
        public void PrintToString_ExcludeTypeMembersFromNestingString_OnExcludeType()
        {
            var person = new Person {Name = "Alex", Age = 19, Parent = new Person {Name = "Andrew", Age = 90}};

            sut.Excluding<string>().PrintToString(person).Should().NotContainAny("Name");
            sut.Excluding<Guid>().PrintToString(person).Should().NotContainAny("Id");
            sut.Excluding<int>().PrintToString(person).Should().NotContainAny("Age");
        }
        
        [Test]
        public void PrintToString_ExcludesMembersFromString_OnExcludeMember()
        {
            var person = new Person {Name = "Alex", Age = 19};

            sut.Excluding(x => x.Age).PrintToString(person).Should().NotContainAny("Age");
            sut.Excluding(x => x.Id).PrintToString(person).Should().NotContainAny("Id");
            sut.Excluding(x => x.Name).PrintToString(person).Should().NotContainAny("Name");
        }
        
        [Test]
        public void PrintToString_ExcludesMembersFromNestingString_OnExcludeMember()
        {
            var person = new Person {Name = "Alex", Age = 19};

            sut.Excluding(x => x.Age).PrintToString(person).Should().NotContainAny("Age");
            sut.Excluding(x => x.Id).PrintToString(person).Should().NotContainAny("Id");
            sut.Excluding(x => x.Name).PrintToString(person).Should().NotContainAny("Name");
        }
        
        [Test]
        public void PrintToString_StringWithAppliedCulture_OnProvidingCultureToMember()
        {
            var person = new Person {Name = "Alex", Age = 19, Height = 1.19};
            
            sut.Printing(x => x.Height)
                .Using(CultureInfo.InvariantCulture)
                .PrintToString(person)
                .Should()
                .Contain("1.19");
            
            sut.Printing(x => x.Height)
                .Using(CultureInfo.GetCultureInfo("ru"))
                .PrintToString(person)
                .Should()
                .Contain("1,19");
        }
        
        [Test]
        public void PrintToString_StringWithAppliedCulture_OnProvidingCultureToType()
        {
            var person = new Person {Name = "Alex", Age = 19, Height = 1.19};
            
            sut.Printing<double>()
                .Using(CultureInfo.InvariantCulture)
                .PrintToString(person)
                .Should()
                .Contain("1.19");
            
            sut.Printing<double>()
                .Using(CultureInfo.GetCultureInfo("ru"))
                .PrintToString(person)
                .Should()
                .Contain("1,19");
        }
        
        [Test]
        public void PrintToString_StringWithCustomTypeSerialize_OnProvidingTypeSerializer()
        {
            var person = new Person {Name = "Alex", Age = 19};
            
            sut.Printing<int>()
                .Using(x => x > 18 ? "Старый" : "Молодой")
                .PrintToString(person)
                .Should()
                .ContainAny("Старый");
        }
        
        [Test]
        public void PrintToString_StringWithCustomTypeSerialize_OnProvidingMemberSerializer()
        {
            var person = new Person {Name = "Alex", Age = 19};
            
            sut.Printing(x => x.Age)
                .Using(x => x > 18 ? "Старый" : "Молодой")
                .PrintToString(person)
                .Should()
                .ContainAny("Старый");
        }
    }
}