using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class PrintingExcludeTests
    {
        private Person testPerson;


        [SetUp]
        public void SetUp()
        {
            testPerson = new Person("John", 180, 32);
        }

        [Test]
        public void Exclude_ShouldExcludeTypes()
        {
            var resString = testPerson.PrintToString(config => config.Excluding<int>());
            resString.Should().Be($"Person{Environment.NewLine}" +
                                  $"\tName = John{Environment.NewLine}" +
                                  $"\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}" +
                                  $"\tHeight = 180{Environment.NewLine}" +
                                  $"\tPet = null{Environment.NewLine}" +
                                  $"\tChild = null");
        }

        [Test]
        public void Exclude_ShouldExcludeProperties()
        {
            var resString = testPerson.PrintToString(config => config.Excluding(p => p.Name));
            resString.Should().Be($"Person{Environment.NewLine}" +
                                  $"\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}" +
                                  $"\tHeight = 180{Environment.NewLine}" +
                                  $"\tAge = 32{Environment.NewLine}" +
                                  $"\tPet = null{Environment.NewLine}"+
                                  $"\tChild = null"
                                  );
        }

        [Test]
        public void Exclude_ShouldNotExcludeThisPropertyInNestedTypes()
        {
            testPerson.Pet = new Pet("Lassie");
            var resString = testPerson.PrintToString(config => config.Excluding(p => p.Name));
            resString.Should()
                .Be($"Person{Environment.NewLine}" +
                    $"\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}" +
                    $"\tHeight = 180{Environment.NewLine}" +
                    $"\tAge = 32{Environment.NewLine}" +
                    $"\tPet = Pet{Environment.NewLine}" +
                    $"\t\tName = Lassie{Environment.NewLine}" +
                    $"\t\tAge = 0{Environment.NewLine}" +
                    $"\tChild = null");

        }

        [Test]
        public void Exclude_ShouldNotExcludeThisTypeInNestedTypes()
        {
            testPerson.Pet = new Pet("Lassie") { Age = 8 };
            var resString = testPerson.PrintToString(config => config.Excluding<int>());
            resString.Should()
                .Be($"Person{Environment.NewLine}" +
                    $"\tName = John{Environment.NewLine}" +
                    $"\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}" +
                    $"\tHeight = 180{Environment.NewLine}" +
                    $"\tPet = Pet{Environment.NewLine}" +
                    $"\t\tName = Lassie{Environment.NewLine}" +
                    $"\t\tAge = 8{Environment.NewLine}" +
                    $"\tChild = null");
        }
    }
}
