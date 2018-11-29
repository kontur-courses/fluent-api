using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    public class Tsar
    {
        public Tsar Son { get; set; }
    }
    [TestFixture]
    public class PrintingTests
    {
        private Person testPerson;

        [SetUp]
        public void SetUp()
        {
            testPerson = new Person("John", 180, 32);
        }

        [Test]
        public void Printing_ShouldAllowToSetPrintMethodForType()
        {
            var resString = testPerson.PrintToString(config => config.Printing<string>().Using(s => s.ToUpper()));
            resString.Should().Be($"Person{Environment.NewLine}" +
                                  $"\tName = JOHN{Environment.NewLine}" +
                                  $"\tHeight = 180{Environment.NewLine}" +
                                  $"\tAge = 32{Environment.NewLine}" +
                                  $"\tPet = null{Environment.NewLine}"+
                                  $"\tChild = null");
        }

        [Test]
        public void Printing_ShouldAllowToSetPrintMethodForProperty()
        {
            var resString = testPerson.PrintToString(config => config.Printing(p => p.Age).Using(age => "0"));
            resString.Should().Be($"Person{Environment.NewLine}" +
                                  $"\tName = John{Environment.NewLine}" +
                                  $"\tHeight = 180{Environment.NewLine}" +
                                  $"\tAge = 0{Environment.NewLine}" +
                                  $"\tPet = null{Environment.NewLine}"+
                                  $"\tChild = null");
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
                                  $"\tPet = null{Environment.NewLine}+" +
                                  $"\tChild = null");
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
                                  $"\tPet = null{Environment.NewLine}" +
                                  $"\tChild = null");
        }

        [Test]
        public void Printing_ShouldAllowToSetLengthInfoForStringsByPropertyName()
        {
            var resString = testPerson.PrintToString(config => config.Printing(p => p.Name).TrimmedToLength(1));
            resString.Should().Be($"Person{Environment.NewLine}" +
                                  $"\tName = J{Environment.NewLine}" +
                                  $"\tHeight = 180{Environment.NewLine}" +
                                  $"\tAge = 32{Environment.NewLine}" +
                                  $"\tPet = null{Environment.NewLine}"
                                  + $"\tChild = null");
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
                    $"\tPet = null{Environment.NewLine}"+
                    $"\tChild = null");
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

        [Test]
        public void SetMaxNestingLevel_ShouldSetNestingLevel()
        {
            var tsar = new Tsar();
            tsar.Son = new Tsar();
            tsar.Son.Son = new Tsar();
            tsar.Son.Son.Son = new Tsar();
            tsar.Son.Son.Son.Son = new Tsar();

            var resString = tsar.PrintToString(config => config.SetMaxNestingLevel(1));
            resString.Should()
                .Be($"Tsar{Environment.NewLine}" +
                    $"\tSon = Tsar (Max nesting level!)");
        }


    }
}