using System;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinter.ObjectPrinter;

namespace ObjectPrinterTests
{
    public class Tests
    {
        private PrintingConfig<Person>? _printerConfig;
        private Person? _mom;
        private Person? _dad;
        private Person? _examplePerson;

        [SetUp]
        public void Setup()
        {
            _printerConfig = new PrintingConfig<Person>();
            _mom = new Person(null, null, Guid.NewGuid(), "Anna", "Karenina", 175.0, 42);
            _dad = new Person(null, null, Guid.NewGuid(), "Bob", "Rockwood", 183.47, 53);
            _examplePerson = new Person(_mom, _dad, Guid.NewGuid(), "Bill", "Gates", 160.5, 15);

        }
        
        [TestCase("ru", ",")]
        [TestCase("en", ".")]
        public void PrintToString_ShouldGiveCorrectFloatingPointDelimiter_WhenSetCulture(string culture, string delimiter)
        {
            var printerConfig = new PrintingConfig<double>();
            
            printerConfig = printerConfig.Printing<double>().Using(new CultureInfo(culture));
            var result = printerConfig.PrintToString(1.1d);
            
            result.Should().Contain(delimiter);
        }
        
        [Test]
        public void PrintToString_CanParseTypeWithCustomSerializer()
        {
            _printerConfig = _printerConfig!.Printing<string>().Using(s => string.Join("", s.Select(_ => '*')));
            var result = _printerConfig.PrintToString(_examplePerson!);
            
            result.Should().ContainAll(
                $"\tId = {_examplePerson.Id}",
                "\tHeight = 160,5",
                "\tAge = 15",
                "\tmother = Person",
                $"\t\tId = {_examplePerson.mother!.Id}",
                "\t\tmother = null",
                "\tfather = Person",
                "\tName = ****",
                "\tSurname = *****",
                "\tSurname = ********");
        }
        
        [Test]
        public void PrintToString_CanParseMemberWithCustomSerializer()
        {
            _printerConfig = _printerConfig!.Printing(p => p.Surname).Using(s => string.Join("", s.Select(_ => '*')));
            var result = _printerConfig.PrintToString(_examplePerson!);
            
            result.Should().ContainAll(
                $"\tId = {_examplePerson.Id}",
                "\tHeight = 160,5",
                "\tAge = 15",
                "\tmother = Person",
                $"\t\tId = {_examplePerson.mother!.Id}",
                "\t\tmother = null",
                "\tfather = Person",
                "\tName = Bill",
                "\tSurname = *****",
                "\tSurname = ********");
        }
        
        [Test]
        public void PrintToString_ShouldExcludeMember_WhenExcluding()
        {
            _printerConfig = _printerConfig!.Excluding(p => p.Surname);
            var result = _printerConfig.PrintToString(_examplePerson!);

            result.Should().NotContain("Surname = ");
            result.Should().Contain("Name = ");
        }

        [Test]
        public void PrintToString_ShouldExcludeType_WhenExcluding()
        {
            _printerConfig = _printerConfig!.Excluding<string>();
            var result = _printerConfig.PrintToString(_examplePerson!);

            result.Should().NotContainAll("Name = ", "Surname = ");
        }

        [Test]
        public void PrintToString_ShouldParseFieldsAndProperties()
        {
            _printerConfig!.PrintToString(_examplePerson!).Should().ContainAll(
                $"\tId = {_examplePerson.Id}",
                "\tHeight = 160,5",
                "\tAge = 15",
                "\tmother = Person",
                $"\t\tId = {_examplePerson.mother!.Id}",
                "\t\tmother = null",
                "\tfather = Person",
                "\tName = Bill",
                "\tSurname = Gates");
        }
        
        [Test]
        public void PrintToString_ShouldTrimParsedString()
        {
            _printerConfig = _printerConfig!.Printing(p => p.Id).UsingTrim(6);
            var result = _printerConfig.PrintToString(_examplePerson!);

            result.Should().NotContainAll($"{_examplePerson.Id}", $"{_examplePerson.mother!.Id}");
            result.Should().ContainAll(
                $"\tId = {string.Join("", _examplePerson.Id.ToString().Take(6))}",
                $"\t\tId = {string.Join("", _examplePerson.mother!.Id.ToString().Take(6))}");
        }
    }
}