using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class PrintingConfigTests
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
                                  $"\tHeight = 180{Environment.NewLine}" +
                                  $"\tPet = null{Environment.NewLine}");
        }

        [Test]
        public void Exclude_ShouldExcludeProperties()
        {
            var resString = testPerson.PrintToString(config => config.Excluding(p => p.Name));
            resString.Should().Be($"Person{Environment.NewLine}" +
                                  $"\tHeight = 180{Environment.NewLine}" +
                                  $"\tAge = 32{Environment.NewLine}" +
                                  $"\tPet = null{Environment.NewLine}");
        }

        [Test]
        public void Exclude_ShouldNotExcludeThisPropertyInNestedTypes()
        {
            testPerson.Pet = new Pet("Lassie");
            var resString = testPerson.PrintToString(config => config.Excluding(p => p.Name));
            resString.Should()
                .Be($"Person{Environment.NewLine}" +
                    $"\tHeight = 180{Environment.NewLine}" +
                    $"\tAge = 32{Environment.NewLine}" +
                    $"\tPet = Pet{Environment.NewLine}" +
                    $"\t\tName = Lassie{Environment.NewLine}\t\tAge = 0");

        }

        [Test]
        public void Exclude_ShouldNotExcludeThisTypeInNestedTypes()
        {
            testPerson.Pet = new Pet("Lassie") { Age = 8 };
            var resString = testPerson.PrintToString(config => config.Excluding<int>());
            resString.Should()
                .Be($"Person{Environment.NewLine}" +
                    $"\tName = John{Environment.NewLine}" +
                    $"\tHeight = 180{Environment.NewLine}" +
                    $"\tPet = Pet{Environment.NewLine}" +
                    $"\t\tName = Lassie{Environment.NewLine}" +
                    $"\t\tAge = 8");
        }

        [Test]
        public void Printing_ShouldAllowToSetPrintMethodForType()
        {
            var resString = testPerson.PrintToString(config => config.Printing<string>().Using(s => s.ToUpper()));
            resString.Should().Be($"Person{Environment.NewLine}" +
                                  $"\tName = JOHN{Environment.NewLine}" +
                                  $"\tHeight = 180{Environment.NewLine}" +
                                  $"\tAge = 32{Environment.NewLine}" +
                                  $"\tPet = null{Environment.NewLine}");
        }

        [Test]
        public void Printing_ShouldAllowToSetPrintMethodForProperty()
        {
            var resString = testPerson.PrintToString(config => config.Printing(p => p.Age).Using(age => "0"));
            resString.Should().Be($"Person{Environment.NewLine}" +
                                  $"\tName = John{Environment.NewLine}" +
                                  $"\tHeight = 180{Environment.NewLine}" +
                                  $"\tAge = 0{Environment.NewLine}" +
                                  $"\tPet = null{Environment.NewLine}");
        }

        [Test]
        public void Printing_ShouldAllowToSetCultureInfoForNumbersByPropertyName()
        {
            testPerson.Height = 180.7;
            var resString = testPerson.PrintToString(config => config.Printing(p => p.Height).ChangeCultureInfo(CultureInfo.CreateSpecificCulture("zu-Z")));
            resString.Should().Be($"Person{Environment.NewLine}" +
                                  $"\tName = John{Environment.NewLine}" +
                                  $"\tHeight = 180.7{Environment.NewLine}" +
                                  $"\tAge = 32{Environment.NewLine}" +
                                  $"\tPet = null{Environment.NewLine}");
        }

        [Test]
        public void Printing_ShouldAllowToSetCultureInfoForNumbersByType()
        {
            testPerson.Height = 180.7;
            var resString = testPerson.PrintToString(config => config.Printing<double>().ChangeCultureInfo(CultureInfo.CreateSpecificCulture("zu-Z")));
            resString.Should().Be($"Person{Environment.NewLine}" +
                                  $"\tName = John{Environment.NewLine}" +
                                  $"\tHeight = 180.7{Environment.NewLine}" +
                                  $"\tAge = 32{Environment.NewLine}" +
                                  $"\tPet = null\r\n");
        }

        [Test]
        public void Printing_ShouldAllowToSetLengthInfoForStringsByPropertyName()
        {
            var resString = testPerson.PrintToString(config => config.Printing(p => p.Name).TrimmedToLength(1));
            resString.Should().Be($"Person{Environment.NewLine}" +
                                  $"\tName = J{Environment.NewLine}" +
                                  $"\tHeight = 180{Environment.NewLine}" +
                                  $"\tAge = 32{Environment.NewLine}" +
                                  $"\tPet = null{Environment.NewLine}");
        }

        [Test]
        public void Printing_ShouldAllowToSetLengthInfoForStringsByType()
        {
            var resString = testPerson.PrintToString(config => config.Printing<string>().TrimmedToLength(1));
            resString.Should()
                .Be($"Person{Environment.NewLine}" +
                    $"\tName = J{Environment.NewLine}" +
                    $"\tHeight = 180{Environment.NewLine}" +
                    $"\tAge = 32{Environment.NewLine}" +
                    $"\tPet = null{Environment.NewLine}");
        }

        [Test]
        public void Printing_ShouldNotThrowStackOverflowException_ThenHighNestingLevel()
        {
            var tsar = new Tsar();
            tsar.Son = new Tsar();
            tsar.Son.Son = new Tsar();
            tsar.Son.Son.Son = new Tsar();
            tsar.Son.Son.Son.Son = new Tsar();

            var resString = tsar.PrintToString();
            resString.Should()
                .Be($"Tsar{Environment.NewLine}" +
                    $"\tSon = Tsar{Environment.NewLine}" +
                    $"\t\tSon = Tsar{Environment.NewLine}" +
                    $"\t\t\tSon = Tsar (Max nesting level!)");
        }

        public class Tsar
        {
            public Tsar Son { get; set; } 
        }



    }
}