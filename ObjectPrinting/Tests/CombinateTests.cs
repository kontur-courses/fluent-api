using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class CombinateTests
    {
        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person("Name", 22.5,23);
        }

        [Test]
        public void Using_ShouldConfiguratePrintMethodProperties()
        {
            var serialize = person.PrintToString(cnfg => cnfg.Printing(p => p.Name).Using(n => n.ToUpper()).Apply());
            serialize.Should().Be($"Person{Environment.NewLine}" +
                                  $"\tName = NAME{Environment.NewLine}" +
                                  $"\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}" +
                                  $"\tHeight = 22,5{Environment.NewLine}" +
                                  $"\tAge = 23{Environment.NewLine}" +
                                  $"\tPet = null{Environment.NewLine}" +
                                  $"\tChild = null");
        }

        [Test]
        public void Using_ShouldConfiguratePrintMethodTypes()
        {
            var serialize = person.PrintToString(cnfg => cnfg.Printing<string>().Using(s => s.ToUpper()).Apply());
            serialize.Should().Be($"Person{Environment.NewLine}" +
                                  $"\tName = NAME{Environment.NewLine}" +
                                  $"\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}" +
                                  $"\tHeight = 22,5{Environment.NewLine}" +
                                  $"\tAge = 23{Environment.NewLine}" +
                                  $"\tPet = null{Environment.NewLine}" +
                                  $"\tChild = null");
        }

        [Test]
        public void Using_ShouldNotConfiguratePrintMethodNestingTypes()
        {
            person.Child = new Person("Baby", 58, 1);
            var serialize = person.PrintToString(cnfg => cnfg.Printing<string>().Using(s => s.ToUpper()).Apply());
            serialize.Should().Be($"Person{Environment.NewLine}" +
                                  $"\tName = NAME{Environment.NewLine}" +
                                  $"\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}" +
                                  $"\tHeight = 22,5{Environment.NewLine}" +
                                  $"\tAge = 23{Environment.NewLine}" +
                                  $"\tPet = null{Environment.NewLine}" +
                                  $"\tChild = Person{Environment.NewLine}" +
                                  $"\t\tName = Baby{Environment.NewLine}" +
                                  $"\t\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}" +
                                  $"\t\tHeight = 58{Environment.NewLine}" +
                                  $"\t\tAge = 1{Environment.NewLine}" +
                                  $"\t\tPet = null{Environment.NewLine}" +
                                  $"\t\tChild = null");
            
        }

        [Test]
        public void Using_ShouldConfigurePrintMethodPropertiesInSeveralWays()
        {
            person.Child = new Person("Baby", 58, 1);
            var serialize = person.PrintToString(cnfg => cnfg.Printing<string>()
                .Using(s => s.ToUpper())
                .Using(s => $"!{s}!")
                .Apply());
            serialize.Should().Be($"Person{Environment.NewLine}" +
                                  $"\tName = !NAME!{Environment.NewLine}" +
                                  $"\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}" +
                                  $"\tHeight = 22,5{Environment.NewLine}" +
                                  $"\tAge = 23{Environment.NewLine}" +
                                  $"\tPet = null{Environment.NewLine}" +
                                  $"\tChild = Person{Environment.NewLine}" +
                                  $"\t\tName = Baby{Environment.NewLine}" +
                                  $"\t\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}" +
                                  $"\t\tHeight = 58{Environment.NewLine}" +
                                  $"\t\tAge = 1{Environment.NewLine}" +
                                  $"\t\tPet = null{Environment.NewLine}" +
                                  $"\t\tChild = null");

        }

        [Test]
        public void TrimToLenght_ShouldTrimString()
        {
            var serialize = person.PrintToString(cnfg => cnfg.Printing<string>().TrimmedToLength(2).Apply());
            serialize.Should().Be($"Person{Environment.NewLine}" +
                                  $"\tName = Na{Environment.NewLine}" +
                                  $"\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}" +
                                  $"\tHeight = 22,5{Environment.NewLine}" +
                                  $"\tAge = 23{Environment.NewLine}" +
                                  $"\tPet = null{Environment.NewLine}" +
                                  $"\tChild = null");
        }

        [Test]
        public void TrimToLenght_ShouldNotTrimString_ThenStringLenghtIsLongerThenParameter()
        {
            var serialize = person.PrintToString(cnfg => cnfg.Printing<string>().TrimmedToLength(10).Apply());
            serialize.Should().Be($"Person{Environment.NewLine}" +
                                  $"\tName = Name{Environment.NewLine}" +
                                  $"\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}" +
                                  $"\tHeight = 22,5{Environment.NewLine}" +
                                  $"\tAge = 23{Environment.NewLine}" +
                                  $"\tPet = null{Environment.NewLine}" +
                                  $"\tChild = null");
        }

        [Test]
        public void ChangeCultureInfo_ShouldChangeCultureInfoInIFormattableTypes()
        {
            var serialize = person.PrintToString(cnfg => cnfg.Printing<IFormattable>()
                .ChangeCultureInfo(CultureInfo.InvariantCulture)
                .Apply());
             serialize.Should().Be($"Person{Environment.NewLine}" +
                                  $"\tName = Name{Environment.NewLine}" +
                                   $"\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}" +
                                  $"\tHeight = 22,5{Environment.NewLine}" +
                                  $"\tAge = 23{Environment.NewLine}" +
                                  $"\tPet = null{Environment.NewLine}" +
                                  $"\tChild = null");
        }

        [Test]
        [Category("ComplicatedTest")]
        public void Printing_ShouldCorrectPrinting()
        {
            person.Child = new Person("Baby", 58, 1);
            var serialize = person.PrintToString(cnfg => cnfg
                .Excluding<int>()
                .Printing(per => per.Name)
                .Using(n => $"Parent name {n}")
                .Apply()
                .Printing<string>()
                .Using(s => $"!{s}!")
                .Apply());
        
            serialize.Should().Be($"Person{Environment.NewLine}" +
                                  $"\tName = !Parent name Name!{Environment.NewLine}" +
                                  $"\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}" +
                                  $"\tHeight = 22,5{Environment.NewLine}" +
                                  $"\tPet = null{Environment.NewLine}" +
                                  $"\tChild = Person{Environment.NewLine}" +
                                  $"\t\tName = Baby{Environment.NewLine}" +
                                  $"\t\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}" +
                                  $"\t\tHeight = 58{Environment.NewLine}" +
                                  $"\t\tAge = 1{Environment.NewLine}" +
                                  $"\t\tPet = null{Environment.NewLine}" +
                                  $"\t\tChild = null");

        }

        [Test]
        public void ComplicatedTest2()
        {
            person.Child = new Person("Baby", 58, 1);
            var serialize = person.PrintToString(cnfg => cnfg               
                .Printing<int>()
                .Using(n => (n*100).ToString())
                .Apply()
                .Printing(p => p.Name)
                .Using(n => n.ToUpper())
                .Apply());

            serialize.Should().Be($"Person{Environment.NewLine}" +
                                  $"\tName = NAME{Environment.NewLine}" +
                                  $"\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}" +
                                  $"\tHeight = 22,5{Environment.NewLine}" +
                                  $"\tAge = 2300{Environment.NewLine}" +
                                  $"\tPet = null{Environment.NewLine}" +
                                  $"\tChild = Person{Environment.NewLine}" +
                                  $"\t\tName = Baby{Environment.NewLine}" +
                                  $"\t\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}" +
                                  $"\t\tHeight = 58{Environment.NewLine}" +
                                  $"\t\tAge = 1{Environment.NewLine}" +
                                  $"\t\tPet = null{Environment.NewLine}" +
                                  $"\t\tChild = null");

        }









    }
}
